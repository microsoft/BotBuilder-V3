using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
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
                var provider = new StaticCredentialProvider(MicrosoftAppId, MicrosoftAppPassword);
                if (Debugger.IsAttached && String.IsNullOrEmpty(provider.AppId))
                    // then auth is disabled
                    return;
                credentialProvider = provider as ICredentialProvider;
            }
            else
            {
                // if we have setting name, or there is no parameters at all default to default setting name
                var provider = new SettingsCredentialProvider(MicrosoftAppIdSettingName, MicrosoftAppPasswordSettingName);
                if (Debugger.IsAttached && String.IsNullOrEmpty(provider.AppId))
                    // then auth is disabled
                    return;
                credentialProvider = provider as ICredentialProvider;
            }

            ClaimsIdentity identity = null;
            JwtTokenExtractor tokenExtractor = null;

            var parameters = JwtConfig.GetToBotFromChannelTokenValidationParameters((audiences, securityToken, validationParameters) => true);
            tokenExtractor = new JwtTokenExtractor(parameters, OpenIdConfigurationUrl);
            identity = await tokenExtractor.GetIdentityAsync(actionContext.Request);

            // No identity? If we're allowed to, fall back to MSA
            // This code path is used by the emulator
            if (identity == null && !DisableSelfIssuedTokens)
            {
                tokenExtractor = new JwtTokenExtractor(JwtConfig.ToBotFromMSATokenValidationParameters, JwtConfig.ToBotFromMSAOpenIdMetadataUrl);
                identity = await tokenExtractor.GetIdentityAsync(actionContext.Request);
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


            // Still no identity? Fail out.
            if (identity == null)
            {
                tokenExtractor.GenerateUnauthorizedResponse(actionContext);
                return;
            }

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

            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

            // Inside of ASP.NET this is required
            if (HttpContext.Current != null)
                HttpContext.Current.User = Thread.CurrentPrincipal;

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}
