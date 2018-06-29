using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

using Microsoft.Rest;

namespace Microsoft.Bot.Connector
{
    public partial class ConnectorClient
    {
        /// <summary>
        /// Create a new instance of the ConnectorClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="microsoftAppId">Optional. Your Microsoft app id. If null, this setting is read from settings["MicrosoftAppId"]</param>
        /// <param name="microsoftAppPassword">Optional. Your Microsoft app password. If null, this setting is read from settings["MicrosoftAppPassword"]</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, string microsoftAppId = null, string microsoftAppPassword = null, params DelegatingHandler[] handlers)
            : this(baseUri, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), handlers: handlers)
        {
        }

        /// <summary>
        /// Create a new instance of the ConnectorClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED)</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
        }

        /// <summary>
        /// Create a new instances of the ConnectorClient.
        /// </summary>
        /// <param name="baseUri">Base URI for the Connector service</param>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="httpClientHandler">The httpClientHandler used by http client</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED)</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public ConnectorClient(Uri baseUri, MicrosoftAppCredentials credentials, HttpClientHandler httpClientHandler, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, httpClientHandler, handlers)
        {
            this.Credentials = credentials;
        }

        partial void CustomInitialize()
        {
            AddUserAgent(this);
        }
        
        internal static void AddUserAgent<T>(T client) where T : ServiceClient<T>
        {
            client.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Microsoft-BotFramework", "3.1"));
            client.HttpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"(BotBuilder .Net/{GetClientVersion(client)})"));
        }
        
        internal static string GetClientVersion<T>(T client) where T : ServiceClient<T>
        {
            var type = client.GetType();
            var assembly = type.GetTypeInfo().Assembly;
            return assembly.GetName().Version.ToString();
        }
    }
}
