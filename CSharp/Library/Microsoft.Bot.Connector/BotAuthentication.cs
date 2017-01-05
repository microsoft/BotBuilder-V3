using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Connector
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class BotAuthentication : ActionFilterAttribute
    {
        /// <summary>
        /// Microsoft AppId for the bot 
        /// </summary>
        /// <remarks>
        /// Needs to be used with MicrosoftAppPassword.  Ignored if CredentialProviderType is specified.
        /// </remarks>
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Microsoft AppPassword for the bot (needs to be used with MicrosoftAppId)
        /// </summary>
        /// <remarks>
        /// Needs to be used with MicrosoftAppId. Ignored if CredentialProviderType is specified.
        /// </remarks>
        public string MicrosoftAppPassword { get; set; }

        /// <summary>
        /// Name of Setting in web.config which has the Microsoft AppId for the bot 
        /// </summary>
        /// <remarks>
        /// Needs to be used with MicrosoftAppPasswordSettingName. Ignored if CredentialProviderType is specified.
        /// </remarks>
        public string MicrosoftAppIdSettingName { get; set; }

        /// <summary>
        /// Name of Setting in web.config which has the Microsoft App Password for the bot 
        /// </summary>
        /// <remarks>
        /// Needs to be used with MicrosoftAppIdSettingName. Ignored if CredentialProviderType is specified.
        /// </remarks>
        public string MicrosoftAppPasswordSettingName { get; set; }

        public bool DisableEmulatorTokens { get; set; }

        /// <summary>
        /// Type which implements ICredentialProvider interface to allow multiple bot AppIds to be registered for the same endpoint
        /// </summary>
        public Type CredentialProviderType { get; set; }

        public virtual string OpenIdConfigurationUrl { get; set; } = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;



        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var provider = this.GetCredentialProvider();
            var botAuthenticator = new BotAuthenticator(provider, OpenIdConfigurationUrl, DisableEmulatorTokens);
            var identityToken = await botAuthenticator.TryAuthenticateAsync(actionContext.Request, cancellationToken);

            // the request is not authenticated, fail with 401.
            if (!identityToken.Authenticated)
            {
                actionContext.Response = BotAuthenticator.GenerateUnauthorizedResponse(actionContext.Request);
                return;
            }

            botAuthenticator.TrustServiceUrls(identityToken, GetActivities(actionContext));
            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        private IList<Activity> GetActivities(HttpActionContext actionContext)
        {
            var activties = actionContext.ActionArguments.Select(t => t.Value).OfType<Activity>().ToList();
            if (activties.Any())
            {
                return activties;
            }
            else
            {
                var objects =
                    actionContext.ActionArguments.Where(t => t.Value is JObject || t.Value is JArray)
                        .Select(t => t.Value).ToArray();
                if (objects.Any())
                {
                    activties = new List<Activity>();
                    foreach (var obj in objects)
                    {
                        activties.AddRange((obj is JObject) ? new Activity[] { ((JObject)obj).ToObject<Activity>() } : ((JArray)obj).ToObject<Activity[]>());
                    }
                }
            }
            return activties;
        }

        private ICredentialProvider GetCredentialProvider()
        {
            ICredentialProvider credentialProvider = null;
            if (CredentialProviderType != null)
            {
                // if we have a credentialprovider type
                credentialProvider = Activator.CreateInstance(CredentialProviderType) as ICredentialProvider;
                if (credentialProvider == null)
                    throw new ArgumentNullException($"The CredentialProviderType {CredentialProviderType.Name} couldn't be instantiated with no params or doesn't implement ICredentialProvider");
            }
            else if (MicrosoftAppId != null && MicrosoftAppPassword != null)
            {
                // if we have raw values
                credentialProvider = new StaticCredentialProvider(MicrosoftAppId, MicrosoftAppPassword);

            }
            else
            {
                // if we have setting name, or there is no parameters at all default to default setting name
                credentialProvider = new SettingsCredentialProvider(MicrosoftAppIdSettingName, MicrosoftAppPasswordSettingName);
            }
            return credentialProvider;
        }
    }

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
            var response = request.CreateResponse(HttpStatusCode.Unauthorized);
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
                    Trace.TraceWarning("No ServiceUrls added to trusted list");
                }
            }
        }

        internal async Task<IdentityToken> TryAuthenticateAsync(HttpRequestMessage request,
            CancellationToken token)
        {
            // then auth is disabled
            if (await this.credentialProvider.IsAuthenticationDisabledAsync())
            {
                return new IdentityToken(true, null);
            }

            ClaimsIdentity identity = null;
            string appId = null;
            var tokenExtractor = GetTokenExtractor();

            // Try to get identity from token as issued by channel
            identity = await tokenExtractor.GetIdentityAsync(request);
            if (identity != null)
                appId = tokenExtractor.GetAppIdFromClaimsIdentity(identity);

            // No identity? If we're allowed to, fall back to emulator path
            if (identity == null && !this.disableEmulatorTokens)
            {
                tokenExtractor = new JwtTokenExtractor(JwtConfig.ToBotFromEmulatorTokenValidationParameters, JwtConfig.ToBotFromEmulatorOpenIdMetadataUrl);
                identity = await tokenExtractor.GetIdentityAsync(request);

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
                Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

                // Inside of ASP.NET this is required
                if (HttpContext.Current != null)
                    HttpContext.Current.User = Thread.CurrentPrincipal;

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

    internal sealed class IdentityToken
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
