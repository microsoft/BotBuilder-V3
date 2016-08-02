using Microsoft.Rest;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Connector
{
    public class MicrosoftAppCredentials : ServiceClientCredentials
    {
        protected static ConcurrentDictionary<string, DateTime> TrustedHostNames = new ConcurrentDictionary<string, DateTime>(
                                                                                        new Dictionary<string, DateTime>() {
                                                                                            { "state.botframework.com", DateTime.MaxValue }
                                                                                        });

        public MicrosoftAppCredentials(string appId = null, string password = null)
        {
            MicrosoftAppId = appId ?? ConfigurationManager.AppSettings["MicrosoftAppId"];
            MicrosoftAppPassword = password ?? ConfigurationManager.AppSettings["MicrosoftAppPassword"];
            TokenCacheKey = $"{MicrosoftAppId}-cache";
        }

        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppIdSettingName { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string MicrosoftAppPasswordSettingName { get; set; }

        public virtual string OAuthEndpoint { get { return "https://login.microsoftonline.com/common/oauth2/v2.0/token"; } }
        public virtual string OAuthScope { get { return "https://graph.microsoft.com/.default"; } }

        protected readonly string TokenCacheKey;

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
                    TrustedHostNames.AddOrUpdate(new Uri(serviceUrl).Host, DateTime.UtcNow.AddDays(1), (key, oldValue) => DateTime.UtcNow.AddDays(1));
                }
                else
                {
                    TrustedHostNames.AddOrUpdate(new Uri(serviceUrl).Host, expirationTime, (key, oldValue) => expirationTime);
                }
            }
            catch (UriFormatException)
            {
                Trace.TraceWarning($"Service url {serviceUrl} is not a well formed Uri!");
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
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());
            }
            await base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            string token;
            var oAuthToken = (OAuthResponse)System.Web.HttpRuntime.Cache.Get(TokenCacheKey);
            if (oAuthToken != null && !forceRefresh && TokenNotExpired(oAuthToken))
            {
                token = oAuthToken.access_token;
            }
            else
            {
                oAuthToken = await RefreshTokenAsync().ConfigureAwait(false);
                System.Web.HttpRuntime.Cache.Insert(TokenCacheKey,
                                                    oAuthToken,
                                                    null,
                                                    DateTime.UtcNow.AddSeconds(oAuthToken.expires_in),
                                                    System.Web.Caching.Cache.NoSlidingExpiration);
                token = oAuthToken.access_token;
            }
            return token;
        }

        private bool ShouldSetToken(HttpRequestMessage request)
        {
            // There is no current http context, proactive message
            // assuming that developer is not calling drop context
            if (HttpContext.Current == null || TrustedUri(request.RequestUri))
            {
                return true;
            }
            else if (HttpContext.Current.User != null)
            {
                // This check is redundant now because RequestUri should already be in the 
                // trusted uri list added by BotAuthentication attribute
                ClaimsIdentity identity = (ClaimsIdentity)HttpContext.Current.User.Identity;

                if (identity?.Claims.FirstOrDefault(c => c.Type == "appid" && JwtConfig.GetToBotFromChannelTokenValidationParameters(MicrosoftAppId).ValidIssuers.Contains(c.Issuer)) != null)
                    return true;

                // Fallback for BF-issued tokens
                if (identity?.Claims.FirstOrDefault(c => c.Issuer == "https://api.botframework.com" && c.Type == "aud") != null)
                    return true;

                // For emulator, we fallback to MSA as valid issuer
                if (identity?.Claims.FirstOrDefault(c => c.Type == "appid" && JwtConfig.ToBotFromMSATokenValidationParameters.ValidIssuers.Contains(c.Issuer)) != null)
                    return true;
            }

            Trace.TraceWarning($"Service url {request.RequestUri.Authority} is not trusted and JwtToken cannot be sent to it.");
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

        private async Task<OAuthResponse> RefreshTokenAsync()
        {
            MicrosoftAppId = MicrosoftAppId ?? ConfigurationManager.AppSettings[MicrosoftAppIdSettingName ?? "MicrosoftAppId"];
            MicrosoftAppPassword = MicrosoftAppPassword ?? ConfigurationManager.AppSettings[MicrosoftAppPasswordSettingName ?? "MicrosoftAppPassword"];

            OAuthResponse oauthResponse;

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.PostAsync(OAuthEndpoint, new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", MicrosoftAppId },
                    { "client_secret", MicrosoftAppPassword },
                    { "scope", OAuthScope }
                })).ConfigureAwait(false);

                string body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(body);
                oauthResponse.expiration_time = DateTime.UtcNow.AddSeconds(oauthResponse.expires_in).Subtract(TimeSpan.FromSeconds(60));
                return oauthResponse;
            }
        }

        private bool TokenNotExpired(OAuthResponse token)
        {
            return token.expiration_time > DateTime.UtcNow;
        }

        private class OAuthResponse
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
            public DateTime expiration_time { get; set; }
        }
    }
}
