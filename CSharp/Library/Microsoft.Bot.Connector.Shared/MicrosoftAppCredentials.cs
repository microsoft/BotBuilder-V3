using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Newtonsoft.Json;

#if NET45
using System.Configuration;
using System.Diagnostics;
using System.Runtime.Serialization;
#endif

namespace Microsoft.Bot.Connector
{
    public class MicrosoftAppCredentials : ServiceClientCredentials
    {
        /// <summary>
        /// The key for Microsoft app Id.
        /// </summary>
        public const string MicrosoftAppIdKey = "MicrosoftAppId";

        /// <summary>
        /// The key for Microsoft app Password.
        /// </summary>
        public const string MicrosoftAppPasswordKey = "MicrosoftAppPassword";

        protected static ConcurrentDictionary<string, DateTime> TrustedHostNames = new ConcurrentDictionary<string, DateTime>(
                                                                                        new Dictionary<string, DateTime>() {
                                                                                            { "state.botframework.com", DateTime.MaxValue },
                                                                                            { "api.botframework.com", DateTime.MaxValue },
                                                                                            { "token.botframework.com", DateTime.MaxValue }
                                                                                        });

        protected static readonly Dictionary<string, Task<OAuthResponse>> tokenTaskCache = new Dictionary<string, Task<OAuthResponse>>();
        protected static readonly ConcurrentDictionary<string, DateTime> autoRefreshTimes = new ConcurrentDictionary<string, DateTime>();
        protected static readonly ConcurrentDictionary<string, OAuthResponse> tokenCache = new ConcurrentDictionary<string, OAuthResponse>();

#if !NET45
        protected ILogger logger;
#endif 

        public MicrosoftAppCredentials(string appId = null, string password = null)
        {
            MicrosoftAppId = appId;
            MicrosoftAppPassword = password;
#if NET45
            if (appId == null)
            {
                MicrosoftAppId = ConfigurationManager.AppSettings[MicrosoftAppIdKey] ?? Environment.GetEnvironmentVariable(MicrosoftAppIdKey, EnvironmentVariableTarget.Process);
            }

            if (password == null)
            {
                MicrosoftAppPassword = ConfigurationManager.AppSettings[MicrosoftAppPasswordKey] ?? Environment.GetEnvironmentVariable(MicrosoftAppPasswordKey, EnvironmentVariableTarget.Process);
            }
#endif
            CacheKey = $"{MicrosoftAppId}-cache";
        }

#if !NET45
        public MicrosoftAppCredentials(string appId, string password, ILogger logger)
            : this(appId, password)
        {
            this.logger = logger;
        }
#endif

#if !NET45
        public MicrosoftAppCredentials(IConfiguration configuration, ILogger logger = null)
            : this(configuration.GetSection(MicrosoftAppIdKey)?.Value, configuration.GetSection(MicrosoftAppPasswordKey)?.Value, logger)
        {
        }
#endif


        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }

        public virtual string OAuthEndpoint { get { return JwtConfig.ToChannelFromBotLoginUrl; } }
        public virtual string OAuthScope { get { return JwtConfig.ToChannelFromBotOAuthScope; } }

        protected readonly string CacheKey;

        /// <summary>
        /// TimeWindow which controlls how often the token will be automatically updated
        /// </summary>
        public static TimeSpan AutoTokenRefreshTimeSpan { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service url</param>
        /// <param name="expirationTime">The expiration time after which this service url is not trusted anymore</param>
        /// <remarks>If expiration time is not provided, the expiration time will DateTime.UtcNow.AddDays(1).</remarks>
        public static void TrustServiceUrl(string serviceUrl, DateTime expirationTime = default(DateTime))
        {
            try
            {
                if (expirationTime == default(DateTime))
                {
                    // by default the service url is valid for one day
                    var extensionPeriod = TimeSpan.FromDays(1);
                    TrustedHostNames.AddOrUpdate(new Uri(serviceUrl).Host, DateTime.UtcNow.Add(extensionPeriod), (key, oldValue) =>
                    {
                        var newExpiration = DateTime.UtcNow.Add(extensionPeriod);
                        // try not to override expirations that are greater than one day from now
                        if (oldValue > newExpiration)
                        {
                            // make sure that extension can be added to oldValue and ArgumentOutOfRangeException
                            // is not thrown
                            if (oldValue >= DateTime.MaxValue.Subtract(extensionPeriod))
                            {
                                newExpiration = oldValue;
                            }
                            else
                            {
                                newExpiration = oldValue.Add(extensionPeriod);
                            }
                        }
                        return newExpiration;
                    });
                }
                else
                {
                    TrustedHostNames.AddOrUpdate(new Uri(serviceUrl).Host, expirationTime, (key, oldValue) => expirationTime);
                }
            }
            catch (Exception)
            {
#if NET45
                Trace.TraceWarning($"Service url {serviceUrl} is not a well formed Uri!");
#endif
            }
        }

        /// <summary>
        /// Checks if the service url is for a trusted host or not.
        /// </summary>
        /// <param name="serviceUrl">The service url</param>
        /// <returns>True if the host of the service url is trusted; False otherwise.</returns>
        public static bool IsTrustedServiceUrl(string serviceUrl)
        {
            Uri uri;
            if (Uri.TryCreate(serviceUrl, UriKind.Absolute, out uri))
            {
                return TrustedUri(uri);
            }
            return false;
        }

        /// <summary>
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param><param name="cancellationToken">Cancellation token.</param>
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ShouldSetToken(request))
            {
                string token = await this.GetTokenAsync().ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            await base.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            Task<OAuthResponse> oAuthTokenTask = null;
            OAuthResponse oAuthToken = null;

            // get tokenTask from cache 
            lock (tokenTaskCache)
            {
                // if we are being forced or don't have a token in our cache at all
                if (forceRefresh || !tokenCache.TryGetValue(CacheKey, out oAuthToken))
                {
                    // we will await this task, because we don't have a token and we need it
                    oAuthTokenTask = _getCurrentTokenTask(forceRefresh: forceRefresh);
                }
                else
                {
                    // we have an oAuthToken
                    // check to see if our token is expired 
                    if (TokenExpired(oAuthToken))
                    {
                        // it is, we should await the current task (someone could have already asked for a new token)
                        oAuthTokenTask = _getCurrentTokenTask(forceRefresh:false);

                        // if the task is completed and is the expired token, then we need to force a new one 
                        // (This happens if bot has been 100% idle past the expiration point)
                        if (oAuthTokenTask.Status == TaskStatus.RanToCompletion && oAuthTokenTask.Result.access_token == oAuthToken.access_token)
                        {
                            oAuthTokenTask = _getCurrentTokenTask(forceRefresh: true);
                        }
                    }

                    // always check the autorefresh
                    CheckAutoRefreshToken();
                }
            }

            // if we have an oAuthTokenTask then we need to await it
            if (oAuthTokenTask != null)
            {
                oAuthToken = await oAuthTokenTask;
                tokenCache[CacheKey] = oAuthToken;
            }

            return oAuthToken?.access_token;
        }

        private void CheckAutoRefreshToken()
        {
            // get the current autorefreshTime for this key
            DateTime refreshTime;
            if (autoRefreshTimes.TryGetValue(CacheKey, out refreshTime))
            {
                // if we are past the refresh time
                if (DateTime.UtcNow > refreshTime)
                {
                    // set new refresh time (this keeps only one outstanding refresh Task at a time)
                    autoRefreshTimes[CacheKey] = DateTime.UtcNow + AutoTokenRefreshTimeSpan;

                    // background task to refresh the token
                    // NOTE: This is not awaited, but is observed with the ContinueWith() clause.  
                    // It is gated by the AutoRefreshTimes[] array
                    RefreshTokenAsync()
                        .ContinueWith(task =>
                        {
                            // observe the background task and put in cache when done
                            if (task.Status == TaskStatus.RanToCompletion)
                            {
                                // update the cache with completed task so all new requests will get it
                                tokenTaskCache[CacheKey] = task;
                            }
                            else
                            {
                                // it failed, shorten the refresh time for another task to try again in a 30s
                                autoRefreshTimes[CacheKey] = DateTime.UtcNow + TimeSpan.FromSeconds(30);
                            }
                        });
                }
            }
        }

        /// <summary>
        /// Do not call this except from GetTokenRefreshAsync() 
        /// </summary>
        /// <returns></returns>
        private Task<OAuthResponse> _getCurrentTokenTask(bool forceRefresh)
        {
            Task<OAuthResponse> oAuthTokenTask;

            // if there is not a task or we are forcing it
            if (forceRefresh || tokenTaskCache.TryGetValue(CacheKey, out oAuthTokenTask) == false)
            {
                // create it
                oAuthTokenTask = RefreshTokenAsync();
                tokenTaskCache[CacheKey] = oAuthTokenTask;

                // set initial refresh time
                autoRefreshTimes[CacheKey] = DateTime.UtcNow + AutoTokenRefreshTimeSpan;
            }
            // if task is in faulted or canceled state then replace it with another attempt
            else if (oAuthTokenTask.IsFaulted || oAuthTokenTask.IsCanceled)
            {
                oAuthTokenTask = RefreshTokenAsync();
                tokenTaskCache[CacheKey] = oAuthTokenTask;

                // set initial refresh time
                autoRefreshTimes[CacheKey] = DateTime.UtcNow + AutoTokenRefreshTimeSpan;
            }

            return oAuthTokenTask;
        }

        private bool ShouldSetToken(HttpRequestMessage request)
        {
            if (TrustedUri(request.RequestUri))
            {
                return true;
            }

#if NET45
            Trace.TraceWarning($"Service url {request.RequestUri.Authority} is not trusted and JwtToken cannot be sent to it.");
#else
            logger?.LogWarning($"Service url {request.RequestUri.Authority} is not trusted and JwtToken cannot be sent to it.");
#endif
            return false;
        }

        private static bool TrustedUri(Uri uri)
        {
            DateTime trustedServiceUrlExpiration;
            if (TrustedHostNames.TryGetValue(uri.Host, out trustedServiceUrlExpiration))
            {
                // check if the trusted service url is still valid
                if (trustedServiceUrlExpiration > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                {
                    return true;
                }
            }
            return false;
        }

#if NET45
        [Serializable]
#endif
        public sealed class OAuthException : Exception
        {
            public OAuthException(string body, Exception inner)
                : base(body, inner)
            {
            }

#if NET45
            private OAuthException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
#endif
        }

        private static HttpClient g_httpClient = new HttpClient();

        private async Task<OAuthResponse> RefreshTokenAsync()
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", MicrosoftAppId },
                    { "client_secret", MicrosoftAppPassword },
                    { "scope", OAuthScope }
                });

            using (var response = await g_httpClient.PostAsync(OAuthEndpoint, content).ConfigureAwait(false))
            {
                string body = null;
                try
                {
                    response.EnsureSuccessStatusCode();
                    body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(body);
                    oauthResponse.expiration_time = DateTime.UtcNow.AddSeconds(oauthResponse.expires_in).Subtract(TimeSpan.FromSeconds(60));
                    return oauthResponse;
                }
                catch (Exception error)
                {
                    throw new OAuthException(body ?? response.ReasonPhrase, error);
                }
            }
        }

        private bool TokenExpired(OAuthResponse token)
        {
            return DateTime.UtcNow > token.expiration_time;
        }

        private bool TokenOld(OAuthResponse token)
        {
            var halfwayExpiration = (token.expiration_time - TimeSpan.FromSeconds(token.expires_in / 2));
            return DateTime.UtcNow > halfwayExpiration;
        }

        protected class OAuthResponse
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public DateTime expiration_time { get; set; }
        }
    }
}
