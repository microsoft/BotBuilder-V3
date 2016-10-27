using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

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

        public bool DisableSelfIssuedTokens { get; set; }

        /// <summary>
        /// Type which implements ICredentialProvider interface to allow multiple bot AppIds to be registered for the same endpoint
        /// </summary>
        public Type CredentialProviderType { get; set; }

        public virtual string OpenIdConfigurationUrl { get; set; } = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var provider = this.GetCredentialProvider();
            var botAuthenticator = new BotAuthenticator(provider, OpenIdConfigurationUrl, DisableSelfIssuedTokens);
            var identityToken = await botAuthenticator.TryAuthenticate(actionContext.Request, cancellationToken);

            // no authenticated Fail out.
            if (!identityToken.Authenticed)
            {
                actionContext.Response = JwtTokenExtractor.GenerateUnauthorizedResponse(actionContext.Request);
                return;
            }

            // authenticated but no identity, auth is disabled;
            if (identityToken.Authenticed && identityToken.Identity == null)
            {
                await base.OnActionExecutingAsync(actionContext, cancellationToken);
                return;
            }

            // trust the service url
            var activity = actionContext.ActionArguments.Select(t => t.Value).OfType<Activity>().FirstOrDefault();
            if (activity != null)
            {
                MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
            }
            else
            {
                // No model binding to activity check if we can find JObject or JArray
                var obj = actionContext.ActionArguments.Where(t => t.Value is JObject || t.Value is JArray).Select(t => t.Value).FirstOrDefault();
                if (obj != null)
                {
                    Activity[] activities = (obj is JObject) ? new Activity[] { ((JObject)obj).ToObject<Activity>() } : ((JArray)obj).ToObject<Activity[]>();
                    foreach (var jActivity in activities)
                    {
                        if (!string.IsNullOrEmpty(jActivity.ServiceUrl))
                        {
                            MicrosoftAppCredentials.TrustServiceUrl(jActivity.ServiceUrl);
                        }
                    }
                }
                else
                {
                    Trace.TraceWarning("No activity in the Bot Authentication Action Arguments");
                }
            }

            Thread.CurrentPrincipal = new ClaimsPrincipal(identityToken.Identity);

            // Inside of ASP.NET this is required
            if (HttpContext.Current != null)
                HttpContext.Current.User = Thread.CurrentPrincipal;

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
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
        private readonly bool disableSelfIssuedTokens;

        /// <summary>
        /// Creates an instance of bot authenticator. 
        /// </summary>
        /// <param name="MicrosoftAppId"> The Microsoft app Id.</param>
        /// <param name="MicrosoftAppPassword"> The Microsoft app password.</param>
        /// <remarks> This constructor sets the <see cref="openIdConfigurationUrl"/> to 
        /// <see cref="JwtConfig.ToBotFromChannelOpenIdMetadataUrl"/>  and doesn't disable 
        /// the self issued tokens used by emulator.
        /// </remarks>
        public BotAuthenticator(string MicrosoftAppId, string MicrosoftAppPassword)
        {
            this.credentialProvider = new StaticCredentialProvider(MicrosoftAppId, MicrosoftAppPassword);
            this.openIdConfigurationUrl = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;
            // self issued tokens are used by emulator
            this.disableSelfIssuedTokens = false;
        }

        public BotAuthenticator(ICredentialProvider credentialProvider, string openIdConfigurationUrl,
            bool disableSelfIssuedTokens)
        {
            if (credentialProvider == null)
            {
                throw new ArgumentNullException(nameof(credentialProvider));
            }
            this.credentialProvider = credentialProvider;
            this.openIdConfigurationUrl = openIdConfigurationUrl;
            this.disableSelfIssuedTokens = disableSelfIssuedTokens;

        }

        /// <summary>
        /// Authenticates the incoming request and add the <see cref="IActivity.ServiceUrl"/> for each
        /// activities to <see cref="MicrosoftAppCredentials.TrustedHostNames"/> if the request is authenticated.
        /// </summary>
        /// <param name="request"> The request that should be authenticated.</param>
        /// <param name="activities"> The activities extracted from request.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> TryAuthenticate(HttpRequestMessage request, IEnumerable<IActivity> activities,
           CancellationToken token)
        {
            var identityToken = await this.TryAuthenticate(request, token);
            if (identityToken.Authenticed)
            {
                foreach (var activity in activities)
                {
                    MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
                }

                if (identityToken.Identity != null)
                {
                    Thread.CurrentPrincipal = new ClaimsPrincipal(identityToken.Identity);

                    // Inside of ASP.NET this is required
                    if (HttpContext.Current != null)
                        HttpContext.Current.User = Thread.CurrentPrincipal;
                }
            }
            return identityToken.Authenticed;
        }

        internal async Task<IdentityToken> TryAuthenticate(HttpRequestMessage request,
            CancellationToken token)
        {
            if (Debugger.IsAttached && this.credentialProvider is SimpleCredentialProvider)
            {
                if (String.IsNullOrEmpty(((SimpleCredentialProvider)this.credentialProvider).AppId))
                {
                    // then auth is disabled
                    return new IdentityToken(true, null);
                }
            }

            ClaimsIdentity identity = null;
            var tokenExtractor = GetTokenExtractor();
            identity = await tokenExtractor.GetIdentityAsync(request);

            // No identity? If we're allowed to, fall back to MSA
            // This code path is used by the emulator
            if (identity == null && !this.disableSelfIssuedTokens)
            {
                tokenExtractor = new JwtTokenExtractor(JwtConfig.ToBotFromMSATokenValidationParameters, JwtConfig.ToBotFromMSAOpenIdMetadataUrl);
                identity = await tokenExtractor.GetIdentityAsync(request);
            }

            if (identity != null)
            {
                var appId = tokenExtractor.GetAppIdFromClaimsIdentity(identity);
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
                return new IdentityToken(true, identity);
            }

            return new IdentityToken(false, null);
        }

        private JwtTokenExtractor GetTokenExtractor()
        {
            var parameters = JwtConfig.GetToBotFromChannelTokenValidationParameters((audiences, securityToken, validationParameters) => true);
            return new JwtTokenExtractor(parameters, this.openIdConfigurationUrl);
        }

    }

    internal sealed class IdentityToken
    {
        public readonly bool Authenticed;
        public readonly ClaimsIdentity Identity;

        public IdentityToken(bool authenticated, ClaimsIdentity identity)
        {
            this.Authenticed = authenticated;
            this.Identity = identity;
        }
    }
}
