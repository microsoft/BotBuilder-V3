using System;
using System.Net.Http;

namespace Microsoft.Bot.Connector
{
    public partial class ConnectorClient
    {
        /// <summary>
        /// Create a new instance of the ConnectorClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="appId">Optional. Your app id. If null, this setting is read from settings["AppId"]</param>
        /// <param name="appSecret">Optional. Your app secret. If null, this setting is read from settings["AppSecret"]</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, string appId = null, string appSecret = null, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = new BasicAuthCredentials(appId, appSecret);
        }

        /// <summary>
        /// Create a new instance of the ConnectorClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, BasicAuthCredentials credentials, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
        }

        /// <summary>
        /// Create a new instance of the ConnectorClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, BearerTokenCredentials credentials, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
        }
    }
}
