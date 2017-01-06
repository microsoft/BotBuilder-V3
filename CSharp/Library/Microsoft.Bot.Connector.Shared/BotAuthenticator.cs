using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
#if NET45
using System.Diagnostics;
using System.Web;
#endif


namespace Microsoft.Bot.Connector
{

    public sealed class BotAuthenticator
    {
        private readonly ICredentialProvider credentialProvider;
        private readonly string openIdConfigurationUrl;
        private readonly bool disableEmulatorTokens;

        /// <summary>
        /// Creates an instance of bot authenticator. 
        /// </summary>
        /// <param name="microsoftAppId"> The Microsoft app Id.</param>
        /// <param name="microsoftAppPassword"> The Microsoft app password.</param>
        /// <remarks> This constructor sets the <see cref="openIdConfigurationUrl"/> to 
        /// <see cref="JwtConfig.ToBotFromChannelOpenIdMetadataUrl"/>  and doesn't disable 
        /// the self issued tokens used by emulator.
        /// </remarks>
        public BotAuthenticator(string microsoftAppId, string microsoftAppPassword)
        {
            this.credentialProvider = new StaticCredentialProvider(microsoftAppId, microsoftAppPassword);
            this.openIdConfigurationUrl = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;
            // by default Authenticator is not disabling emulator tokens
            this.disableEmulatorTokens = false;
        }

        public BotAuthenticator(ICredentialProvider credentialProvider)
            : this(credentialProvider, JwtConfig.ToBotFromChannelOpenIdMetadataUrl, false)
        {
        }

        public BotAuthenticator(ICredentialProvider credentialProvider, string openIdConfigurationUrl,
            bool disableEmulatorTokens)
        {
            if (credentialProvider == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }
            this.credentialProvider = credentialProvider;
            this.openIdConfigurationUrl = openIdConfigurationUrl;
            this.disableEmulatorTokens = disableEmulatorTokens;

        }

        /// <summary>
        /// Authenticates the incoming request and add the <see cref="IActivity.ServiceUrl"/> for each
        /// activities to <see cref="MicrosoftAppCredentials.TrustedHostNames"/> if the request is authenticated.
        /// </summary>
        /// <param name="request"> The request that should be authenticated.</param>
        /// <param name="activities"> The activities extracted from request.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> TryAuthenticateAsync(HttpRequestMessage request, IEnumerable<IActivity> activities,
           CancellationToken token)
        {
            var identityToken = await this.TryAuthenticateAsync(request, token);
            TrustServiceUrls(identityToken, activities);
            return identityToken.Authenticated;
        }

        /// <summary>
        /// Generates <see cref="HttpStatusCode.Unauthorized"/> response for the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>A response with status code unauthorized.</returns>
        public static HttpResponseMessage GenerateUnauthorizedResponse(HttpRequestMessage request)
        {
            string host = request.RequestUri.DnsSafeHost;
#if NET45
            var response = request.CreateResponse(HttpStatusCode.Unauthorized);
#else
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
#endif
            response.Headers.Add("WWW-Authenticate", string.Format("Bearer realm=\"{0}\"", host));
            return response;
        }

        internal void TrustServiceUrls(IdentityToken identityToken, IEnumerable<IActivity> activities)
        {
            // add the service url to the list of trusted urls only if the JwtToken 
            // is valid and identity is not null
            if (identityToken.Authenticated && identityToken.Identity != null)
            {
                if (activities.Any())
                {
                    foreach (var activity in activities)
                    {
                        MicrosoftAppCredentials.TrustServiceUrl(activity?.ServiceUrl);
                    }
                }
                else
                {
#if NET45
                    Trace.TraceWarning("No ServiceUrls added to trusted list");
#endif
                }
            }
        }

        internal async Task<IdentityToken> TryAuthenticateAsync(HttpRequestMessage request,
            CancellationToken token)
        {
            var authorizationHeader = request.Headers.Authorization;
            if (authorizationHeader != null)
            {
                return await TryAuthenticateAsync(authorizationHeader.Scheme, authorizationHeader.Parameter, token);
            }
            else if (await this.credentialProvider.IsAuthenticationDisabledAsync())
            {
                return new IdentityToken(true, null);
            }

            return new IdentityToken(false, null);
        }
        
        public async Task<IdentityToken> TryAuthenticateAsync(string scheme, string token,
            CancellationToken cancellationToken)
        {
            // then auth is disabled
            if (await this.credentialProvider.IsAuthenticationDisabledAsync())
            {
                return new IdentityToken(true, null);
            }

            ClaimsIdentity identity = null;
            string appId = null;
            var tokenExtractor = GetTokenExtractor();
            identity = await tokenExtractor.GetIdentityAsync(scheme, token);
            if (identity != null)
                appId = tokenExtractor.GetAppIdFromClaimsIdentity(identity);

            // No identity? If we're allowed to, fall back to MSA
            // This code path is used by the emulator
            if (identity == null && !this.disableEmulatorTokens)
            {
                tokenExtractor = new JwtTokenExtractor(JwtConfig.ToBotFromEmulatorTokenValidationParameters, JwtConfig.ToBotFromEmulatorOpenIdMetadataUrl);
                identity = await tokenExtractor.GetIdentityAsync(scheme, token);
                
                if (identity != null)
                    appId = tokenExtractor.GetAppIdFromEmulatorClaimsIdentity(identity);
            }

            if (identity != null)
            {
                if (await credentialProvider.IsValidAppIdAsync(appId) == false) // keep context
                {
                    // not valid appid, drop the identity
                    identity = null;
                }
                else
                {
                    var password = await credentialProvider.GetAppPasswordAsync(appId); // Keep context
                    if (password != null)
                    {
                        // add password as claim so that it is part of ClaimsIdentity and accessible by ConnectorClient() 
                        identity.AddClaim(new Claim(ClaimsIdentityEx.AppPasswordClaim, password));
                    }
                }
            }

            if (identity != null)
            {
#if NET45
                Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

                // Inside of ASP.NET this is required
                if (HttpContext.Current != null)
                    HttpContext.Current.User = Thread.CurrentPrincipal;
#endif

                return new IdentityToken(true, identity);
            }

            return new IdentityToken(false, null);
        }

        private JwtTokenExtractor GetTokenExtractor()
        {
            var parameters = JwtConfig.ToBotFromChannelTokenValidationParameters;
            return new JwtTokenExtractor(parameters, this.openIdConfigurationUrl);
        }

    }

    public sealed class IdentityToken
    {
        public readonly bool Authenticated;
        public readonly ClaimsIdentity Identity;

        public IdentityToken(bool authenticated, ClaimsIdentity identity)
        {
            this.Authenticated = authenticated;
            this.Identity = identity;
        }
    }
}
