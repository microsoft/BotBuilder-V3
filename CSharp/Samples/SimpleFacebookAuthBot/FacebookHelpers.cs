using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

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

        public async static Task<FacebookAcessToken> ExchangeCodeForAccessToken(ConversationReference conversationReference, string code, string facebookOauthCallback)
        {
            var redirectUri = GetOAuthCallBack(conversationReference, facebookOauthCallback);
            var uri = GetUri("https://graph.facebook.com/v2.3/oauth/access_token",
                Tuple.Create("client_id", FacebookAppId),
                Tuple.Create("redirect_uri", redirectUri),
                Tuple.Create("client_secret", FacebookAppSecret),
                Tuple.Create("code", code)
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

        private static string GetOAuthCallBack(ConversationReference conversationReference, string facebookOauthCallback)
        {
            var uri = GetUri(facebookOauthCallback,
                Tuple.Create("userId", TokenEncoder(conversationReference.User.Id)),
                Tuple.Create("botId", TokenEncoder(conversationReference.Bot.Id)),
                Tuple.Create("conversationId", TokenEncoder(conversationReference.Conversation.Id)),
                Tuple.Create("serviceUrl", TokenEncoder(conversationReference.ServiceUrl)),
                Tuple.Create("channelId", conversationReference.ChannelId)
                );
            return uri.ToString();
        }

        // because of a limitation on the characters in Facebook redirect_uri, we don't use the serialization of the cookie.
        // http://stackoverflow.com/questions/4386691/facebook-error-error-validating-verification-code
        public static string TokenEncoder(string token)
        {
            return HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(token));
        }

        public static string TokenDecoder(string token)
        {
            return Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(token));
        }

        public static string GetFacebookLoginURL(ConversationReference conversationReference, string facebookOauthCallback)
        {
            var redirectUri = GetOAuthCallBack(conversationReference, facebookOauthCallback);
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
                throw new ArgumentException("Unable to deserialize the Facebook response.", ex);
            }
        }

        private static Uri GetUri(string endPoint, params Tuple<string, string>[] queryParams)
        {
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (var queryparam in queryParams)
            {
                queryString[queryparam.Item1] = queryparam.Item2;
            }

            var builder = new UriBuilder(endPoint);
            builder.Query = queryString.ToString();
            return builder.Uri;
        }
    }


}