// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Connector.Shared.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

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

        protected static readonly ConcurrentDictionary<string, TrustedHostInfo> TrustedHostNames = new ConcurrentDictionary<string, TrustedHostInfo>(
                                                                                        new Dictionary<string, TrustedHostInfo>() {
                                                                                            { "state.botframework.com", new TrustedHostInfo() { DateTime = DateTime.MaxValue } },
                                                                                            { "api.botframework.com", new TrustedHostInfo() { DateTime = DateTime.MaxValue } },
                                                                                            { "token.botframework.com", new TrustedHostInfo() { DateTime = DateTime.MaxValue } }
                                                                                        });
        protected class TrustedHostInfo
        {
            public DateTime DateTime { get; set; }

            public string OAuthScope { get; set; }
        }

#if !NET45
        protected ILogger logger;
#endif 

        private readonly Lazy<AdalAuthenticator> authenticator;

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
            authenticator = new Lazy<AdalAuthenticator>(() => new AdalAuthenticator(new ClientCredential(MicrosoftAppId, MicrosoftAppPassword)), LazyThreadSafetyMode.ExecutionAndPublication);
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

        public static string OAuthEndpoint
        {
            get
            {
                string tenant = null;
#if NET45
                // Advanced user only, see https://aka.ms/bots/tenant-restriction
                tenant = SettingsUtils.GetAppSettings("ChannelAuthTenant");
#endif
                var endpointUrl = string.Format(JwtConfig.ToChannelFromBotLoginUrlTemplate, string.IsNullOrEmpty(tenant) ? "botframework.com" : tenant);

                if (Uri.TryCreate(endpointUrl, UriKind.Absolute, out Uri result))
                    return endpointUrl;

                throw new Exception($"Invalid token endpoint: {endpointUrl}");
            }
        }

        public static string OAuthAuthority
        {
            get
            {
                string tenant = null;
#if NET45
                // Advanced user only, see https://aka.ms/bots/tenant-restriction
                tenant = SettingsUtils.GetAppSettings("ChannelAuthTenant");
#endif
                var authority = string.Format(JwtConfig.ConvergedAppAuthority, string.IsNullOrEmpty(tenant) ? "botframework.com" : tenant);

                if (Uri.TryCreate(authority, UriKind.Absolute, out Uri result))
                    return authority;

                throw new Exception($"Invalid token endpoint: {authority}");
            }
        }

        public static string OAuthBotScope { get { return JwtConfig.ToChannelFromBotOAuthScope; } }

        /// <summary>
        /// Adds the host of service url to <see cref="MicrosoftAppCredentials"/> trusted hosts.
        /// </summary>
        /// <param name="serviceUrl">The service url</param>
        /// <param name="expirationTime">The expiration time after which this service url is not trusted anymore</param>
        /// <param name="oauthScope">(optional) The scope to use while retrieving the token.  If Null, 
        /// MicrosoftAppCredentials.OAuthBotScope will be used.</param>
        /// <remarks>If expiration time is not provided, the expiration time will DateTime.UtcNow.AddDays(1).</remarks>
        public static void TrustServiceUrl(string serviceUrl, DateTime expirationTime = default(DateTime), string oauthScope = null)
        {
            try
            {
                var setExpirationTime = expirationTime;
                if (expirationTime == default(DateTime))
                {
                    // by default the service url is valid for one day
                    setExpirationTime = DateTime.UtcNow.Add(TimeSpan.FromDays(1));
                }

                if (!serviceUrl.EndsWith("/"))
                {
                    serviceUrl += "/";
                }

                TrustedHostNames.AddOrUpdate(serviceUrl,
                                            new TrustedHostInfo
                                            {
                                                DateTime = setExpirationTime,
                                                OAuthScope = oauthScope
                                            }, (key, currentValue) => 
                {                      
                    // If the developer has provided the expiration, use it.
                    // Otherwise, do not overwite newer dates with older dates.
                    if (expirationTime != default(DateTime) || currentValue.DateTime < setExpirationTime)
                    {
                        currentValue.DateTime = setExpirationTime;
                    }

                    if (!string.IsNullOrEmpty(oauthScope))
                    {
                        currentValue.OAuthScope = oauthScope;
                    }

                    return currentValue;
                });
                
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
            var trustedHostInfo = GetTrustedHostInfo(request.RequestUri);
            if (trustedHostInfo != null)
            {
                var oauthScope = trustedHostInfo.OAuthScope;
                var authResult = await GetTokenAsync(oauthScope: oauthScope).ConfigureAwait(false);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult);
            }
            else
            {
#if NET45
                Trace.TraceWarning($"Service url {request.RequestUri.Authority} is not trusted and JwtToken cannot be sent to it.");
#else
            logger?.LogWarning($"Service url {request.RequestUri.Authority} is not trusted and JwtToken cannot be sent to it.");
#endif
            }

            await base.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> GetTokenAsync(bool forceRefresh = false, string oauthScope = null)
        {
            var token = await authenticator.Value.GetTokenAsync(forceRefresh, oauthScope).ConfigureAwait(false);
            return token.AccessToken;
        }

        private void LogWarning(string warning)
        {
#if NET45
            Trace.TraceWarning(warning);
#else
            logger?.LogWarning(warning);
#endif
        }

        private void LogError(string error)
        {
#if NET45
            Trace.TraceError(error);
#else
            logger?.LogError(error);
#endif
        }

        private static TrustedHostInfo GetTrustedHostInfo(Uri uri)
        {
            TrustedHostInfo trustedHostInfo;
            if (TrustedHostNames.TryGetValue(uri.Host, out trustedHostInfo))
            {
                // check if the trusted service url is still valid
                if (trustedHostInfo.DateTime > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                {
                    return trustedHostInfo;
                }
            }

            var endOfBaseUrl = uri.AbsoluteUri.IndexOf("v3/conversations", StringComparison.OrdinalIgnoreCase);
            if (endOfBaseUrl > 0)
            {
                var serviceUrl = uri.AbsoluteUri.Substring(0, endOfBaseUrl);
                if (TrustedHostNames.TryGetValue(serviceUrl, out trustedHostInfo))
                {
                    // check if the trusted service url is still valid
                    if (trustedHostInfo.DateTime > DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(5)))
                    {
                        return trustedHostInfo;
                    }
                }

            }

            return null;
        }

        private static bool TrustedUri(Uri uri)
        {
            return GetTrustedHostInfo(uri) != null;
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
    }
}
