using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Connector
{
    public class MicrosoftAppCredentials : BearerTokenCredentials
    {
        public MicrosoftAppCredentials(string appId = null, string password = null)
            :base(null)
        {
            MicrosoftAppId = appId;
            MicrosoftAppPassword = password;
        }

        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppIdSettingName { get; set; }
        public string MicrosoftAppPassword { get; set; }
        public string MicrosoftAppPasswordSettingName { get; set; }

        public virtual string OAuthEndpoint { get { return "https://login.microsoftonline.com/common/oauth2/v2.0/token"; } }
        public virtual string OAuthScope { get { return "https://graph.microsoft.com/.default"; } }

        public async Task RefreshTokenAsync()
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
                Token = oauthResponse.access_token;
            }
        }

        private class OAuthResponse
        {
            public string token_type { get; set; }
            public int expires_in { get; set; }
            public string access_token { get; set; }
        }
    }
}
