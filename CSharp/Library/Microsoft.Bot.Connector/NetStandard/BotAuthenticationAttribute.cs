using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.Bot.Connector
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class BotAuthenticationAttribute : ActionFilterAttribute
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

        public async override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var provider = this.GetCredentialProvider();
            var botAuthenticator = new BotAuthenticator(provider, GetOpenIdConfigurationUrl(), DisableEmulatorTokens);
            try
            {
                var identityToken = await botAuthenticator.AuthenticateAsync(context.HttpContext.Request,
                    GetActivities(context), context.HttpContext.RequestAborted);
                // the request is not authenticated, fail with 401.
                if (!identityToken.Authenticated)
                {
                    context.Result = BotAuthenticator.GenerateUnauthorizedResponse(context.HttpContext, "BotAuthenticator failed to authenticate incoming request!");
                    return;
                }

            }
            catch (Exception e)
            {
                context.Result = BotAuthenticator.GenerateUnauthorizedResponse(context.HttpContext, $"Failed authenticating incoming request: {e.ToString()}");
                return;
            }
            await next();
        }

        public static IList<Activity> GetActivities(ActionExecutingContext actionContext)
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

        private readonly static Lazy<string> settingsOpenIdConfigurationurl = new Lazy<string>(() => SettingsUtils.GetAppSettings("BotOpenIdMetadata"));

        private string GetOpenIdConfigurationUrl()
        {
            var settingsUrl = settingsOpenIdConfigurationurl.Value;
            return string.IsNullOrEmpty(settingsUrl) ? this.OpenIdConfigurationUrl : settingsUrl;
        }
    }
}
