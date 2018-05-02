using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

#if NET45
using System.Security.Claims;
using System.Threading;
using System.Web;
#endif

namespace Microsoft.Bot.Connector
{
    public partial class OAuthClient
    {
        /// <summary>
        /// Create a new instance of the OAuthClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the OAuth API service</param>
        /// <param name="microsoftAppId">Optional. Your Microsoft app id. If null, this setting is read from settings["MicrosoftAppId"]</param>
        /// <param name="microsoftAppPassword">Optional. Your Microsoft app password. If null, this setting is read from settings["MicrosoftAppPassword"]</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public OAuthClient(Uri baseUri, string microsoftAppId = null, string microsoftAppPassword = null, params DelegatingHandler[] handlers)
            : this(baseUri, new MicrosoftAppCredentials(microsoftAppId, microsoftAppPassword), handlers: handlers)
        {
        }

        /// <summary>
        /// Create a new instance of the OAuthClient class
        /// </summary>
        /// <param name="baseUri">Base URI for the OAuth API service</param>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED)</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public OAuthClient(Uri baseUri, MicrosoftAppCredentials credentials, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(baseUri, handlers)
        {
            this.Credentials = credentials;
        }

#if NET45
        /// <summary>
        /// Create a new instance of the OAuthClient class using Credential source
        /// </summary>
        /// <param name="baseUri">Base URI for the OAuth API service</param>
        /// <param name="credentialProvider">Credential source to use</param>
        /// <param name="claimsIdentity">ClaimsIDentity to create the client for</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public OAuthClient(Uri baseUri, ICredentialProvider credentialProvider, ClaimsIdentity claimsIdentity = null, params DelegatingHandler[] handlers)
            : this(baseUri, handlers: handlers)
        {
            if (claimsIdentity == null)
                claimsIdentity = HttpContext.Current.User.Identity as ClaimsIdentity ?? Thread.CurrentPrincipal.Identity as ClaimsIdentity;

            if (claimsIdentity == null)
                throw new ArgumentNullException("ClaimsIdentity not passed in and not available via Thread.CurrentPrincipal.Identity");

            var appId = claimsIdentity.GetAppIdFromClaims();
            var password = credentialProvider.GetAppPasswordAsync(appId).Result;
            this.Credentials = new MicrosoftAppCredentials(appId, password);
        }

        /// <summary>
        /// Create a new instance of the OAuthClient class using Credential source
        /// </summary>
        /// <param name="credentialProvider">Credential source to use</param>
        /// <param name="claimsIdentity">ClaimsIDentity to create the client for</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public OAuthClient(ICredentialProvider credentialProvider, ClaimsIdentity claimsIdentity = null, params DelegatingHandler[] handlers)
            : this(null, credentialProvider, claimsIdentity, handlers: handlers)
        {
        }
#endif

        /// <summary>
        /// Create a new instance of the OAuthClient class
        /// </summary>
        /// <remarks> This constructor will use https://state.botframework.com as the baseUri</remarks>
        /// <param name="credentials">Credentials for the Connector service</param>
        /// <param name="addJwtTokenRefresher">(DEPRECATED)</param>
        /// <param name="handlers">Optional. The delegating handlers to add to the http client pipeline.</param>
        public OAuthClient(MicrosoftAppCredentials credentials, bool addJwtTokenRefresher = true, params DelegatingHandler[] handlers)
            : this(handlers)
        {
            this.Credentials = credentials;
        }

        internal static string GetClientVersion<T>(T client) where T : ServiceClient<T>
        {
            var type = client.GetType();
            var assembly = type.GetTypeInfo().Assembly;
            return assembly.GetName().Version.ToString();
        }

    }
}
