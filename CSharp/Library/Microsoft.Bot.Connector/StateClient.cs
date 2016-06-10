using System;
using System.Net.Http;

namespace Microsoft.Bot.Connector
{
    public partial class StateClient
    {
        /// <summary>
        /// Create a new instance of the StateClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the State service</param>
        /// <param name="appId">Optional. Your app id. If null, this setting is read from settings["AppId"]</param>
        /// <param name="appSecret">Optional. Your app secret. If null, this setting is read from settings["AppSecret"]</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(Uri baseUri, string appId = null, string appSecret = null, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = new BasicAuthCredentials(appId, appSecret);
        }

        /// <summary>
        /// Create a new instance of the StateClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the State service</param>
        /// <param name="credentials">Credentials for the State service</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(Uri baseUri, BasicAuthCredentials credentials, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
        }

        /// <summary>
        /// Create a new instance of the StateClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the State service</param>
        /// <param name="credentials">Credentials for the State service</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public StateClient(Uri baseUri, BearerTokenCredentials credentials, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
        }
    }
}
