using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Rest;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Use credentials from AppSetting "AppId" "AppSecret"
    /// </summary>
    public class BasicAuthCredentials : ServiceClientCredentials
    {
        private static Lazy<string> _appId = new Lazy<string>(() => ConfigurationManager.AppSettings["AppId"]);
        private static Lazy<string> _appSecret = new Lazy<string>(() => ConfigurationManager.AppSettings["AppSecret"]);
        private static Lazy<string> _appEndpoint = new Lazy<string>(() => ConfigurationManager.AppSettings["AppEndpoint"]);

        public string AppId { get; private set; }

        public string AppSecret { get; private set; }

        public string SubscriptionKey { get; private set; }

        public string Authorization { get; private set; }

        public string Endpoint { get; protected set; }

        /// <summary>
        /// Create a new instance of the BasicAuthCredentials class
        /// </summary>
        /// <param name="appId">default will come from Settings["AppId"]</param>
        /// <param name="appSecret">default will come from settings["AppSecret"]</param>
        /// <param name="subscriptionKey"></param>
        public BasicAuthCredentials(string appId = null, string appSecret = null, string subscriptionKey = null)
        {
            this.AppId = appId ?? _appId.Value;
            this.AppSecret = appSecret ?? _appSecret.Value;
            this.SubscriptionKey = subscriptionKey ?? this.AppSecret;
            var byteArray = Encoding.ASCII.GetBytes($"{this.AppId}:{this.AppSecret}");
            this.Authorization = Convert.ToBase64String(byteArray);
            this.Endpoint = _appEndpoint.Value ?? "https://api.botframework.com/";
        }

        /// <summary>
        /// Apply the credentials to the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request.</param><param name="cancellationToken">Cancellation token.</param>
        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", this.SubscriptionKey);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", this.Authorization);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }
}