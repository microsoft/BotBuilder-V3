using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using System.Web;
using System.Security.Claims;

namespace Microsoft.Bot.Connector
{
    public class MicrosoftAppCredentials : BearerTokenCredentials
    {
        public MicrosoftAppCredentials(string appId = null, string password = null)
            : base(null)
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
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param><param name="cancellationToken">Cancellation token.</param>
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (ShouldSetToken())
            {
                Token = await GetTokenAsync();
            }
            else
            {
                Token = null;
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

        private bool ShouldSetToken()
        {
            // There is no current http context, proactive message
            // assuming that developer is not calling drop context
            if (HttpContext.Current == null)
            {
                return true;
            }
            else if (HttpContext.Current.User != null)
            {
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
