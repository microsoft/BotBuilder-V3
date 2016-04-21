using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.SimpleFacebookAuthBot
{
    public class FacebookAcessToken
    {
        public FacebookAcessToken()
        {
        }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public long ExpiresIn { get; set; }
    }

    class FacebookProfile
    {
        public FacebookProfile()
        {
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
    }

    /// <summary>
    /// Helpers implementing Facebook API calls.
    /// </summary>
    public static class FacebookHelpers
    {
        // The Facebook App Id
        public static readonly string FacebookAppId = "YOUR_FACEBOOK_APP_ID";

        // The Facebook App Secret
        public static readonly string FacebookAppSecret = "YOUR_FACEBOOK_APP_SECRET";

        public async static Task<FacebookAcessToken> ExchangeCodeForAccessToken(string userId, string conversationId, string code, string facebookOauthCallback)
        {
            var redirectUri = GetOAuthCallBack(userId, conversationId, facebookOauthCallback);
            var uri = GetUri("https://graph.facebook.com/v2.3/oauth/access_token",
                Tuple.Create("client_id", FacebookAppId),
                Tuple.Create("client_secret", FacebookAppSecret),
                Tuple.Create("code", code),
                Tuple.Create("redirect_uri", redirectUri)
                );

            return await FacebookRequest<FacebookAcessToken>(uri);
        }

        public static async Task<bool> ValidateAccessToken(string accessToken)
        {
            var uri = GetUri("https://graph.facebook.com/debug_token",
                Tuple.Create("input_token", accessToken),
                Tuple.Create("access_token", $"{FacebookAppId}|{FacebookAppSecret}"));

            var res = await FacebookRequest<object>(uri).ConfigureAwait(false);
            return (((dynamic)res)?.data)?.is_valid;
        }

        public static async Task<string> GetFacebookProfileName(string accessToken)
        {
            var uri = GetUri("https://graph.facebook.com/v2.6/me",
                Tuple.Create("fields", "id,name"),
                Tuple.Create("access_token", accessToken));

            var res = await FacebookRequest<FacebookProfile>(uri);
            return res.Name;
        }

        private static string GetOAuthCallBack(string userId, string conversationId, string facebookOauthCallback)
        {
            var uri = GetUri(facebookOauthCallback,
                Tuple.Create("userId", userId),
                Tuple.Create("conversationId", conversationId));

            return uri.ToString();
        }

        public static string GetFacebookLoginURL(PendingMessage pendingMessage, string facebookOauthCallback)
        {
            var redirectUri = GetOAuthCallBack(pendingMessage.userId, pendingMessage.conversationId, facebookOauthCallback);
            var uri = GetUri("https://www.facebook.com/dialog/oauth",
                Tuple.Create("client_id", FacebookAppId),
                Tuple.Create("redirect_uri", redirectUri),
                Tuple.Create("response_type", "code"),
                Tuple.Create("scope", "public_profile,email"),
                Tuple.Create("state", Convert.ToString(new Random().Next(9999)))
                );

            return uri.ToString();
        }

        private static async Task<T> FacebookRequest<T>(Uri uri)
        {
            string json;
            using (HttpClient client = new HttpClient())
            {
                json = await client.GetStringAsync(uri).ConfigureAwait(false);
            }

            try
            {
                var result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Unable to deserialize the facebook response.", ex);
            }
        }

        private static Uri GetUri(string endPoint, params Tuple<string, string>[] queryParams)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach(var queryparam in queryParams)
            {
                queryString[queryparam.Item1] = queryparam.Item2; 
            }

            var builder = new UriBuilder(endPoint);
            builder.Query = queryString.ToString();
            return builder.Uri; 
        }
    }


}