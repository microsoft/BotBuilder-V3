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
    public class BotAuthentication : AuthorizationFilterAttribute
    {
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppIdSettingName { get; set; }
        public bool DisableSelfIssuedTokens { get; set; }
        public virtual string OpenIdConfigurationUrl { get; set; } = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            MicrosoftAppId = MicrosoftAppId ?? ConfigurationManager.AppSettings[MicrosoftAppIdSettingName ?? "MicrosoftAppId"];

            if (Debugger.IsAttached && String.IsNullOrEmpty(MicrosoftAppId))
                // then auth is disabled
                return;

            var tokenExtractor = new JwtTokenExtractor(JwtConfig.GetToBotFromChannelTokenValidationParameters(MicrosoftAppId), OpenIdConfigurationUrl);
            var identity = await tokenExtractor.GetIdentityAsync(actionContext.Request);

            // No identity? If we're allowed to, fall back to MSA
            // This code path is used by the emulator
            if (identity == null && !DisableSelfIssuedTokens)
            {
                tokenExtractor = new JwtTokenExtractor(JwtConfig.ToBotFromMSATokenValidationParameters, JwtConfig.ToBotFromMSAOpenIdMetadataUrl);
                identity = await tokenExtractor.GetIdentityAsync(actionContext.Request);

                // Check to make sure the app ID in the token is ours
                if (identity != null)
                {
                    // If it doesn't match, throw away the identity
                    if (tokenExtractor.GetBotIdFromClaimsIdentity(identity) != MicrosoftAppId)
                        identity = null;
                }
            }

            // Still no identity? Fail out.
            if (identity == null)
            {
                tokenExtractor.GenerateUnauthorizedResponse(actionContext);
                return;
            }

            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

            // Inside of ASP.NET this is required
            if (HttpContext.Current != null)
                HttpContext.Current.User = Thread.CurrentPrincipal;

            await base.OnAuthorizationAsync(actionContext, cancellationToken);
        }
    }
}