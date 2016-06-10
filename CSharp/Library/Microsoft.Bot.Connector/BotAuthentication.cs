using System;
using System.Configuration;
using System.Linq;
using System.Net;
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
    public class BotAuthentication : AuthorizationFilterAttribute
    {
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppIdSettingName { get; set; }
        public virtual string Issuer { get { return "https://api.botframework.com"; } }
        //TODO: change this back to the production one
        public virtual string OpenIdConfigurationUrl { get { return "https://intercom-api-scratch.azurewebsites.net/api/.well-known/OpenIdConfiguration"; } }

        /// <summary>
        /// Override to Web API filter method to handle Basic Auth check
        /// </summary>
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            MicrosoftAppId = MicrosoftAppId ?? ConfigurationManager.AppSettings[MicrosoftAppIdSettingName ?? "MicrosoftAppId"];

            if (actionContext.Request.RequestUri.Host == "localhost")
                return;

            var tokenExtractor = new JwtTokenExtractor(new string[] { MicrosoftAppId }, new string[] { Issuer }, OpenIdConfigurationUrl);

            // Get the identity from the request
            var identity = await tokenExtractor.GetIdentityAsync(actionContext.Request);
            // Error if we didn't find a valid identity
            if (identity == null)
            {
                var host = actionContext.Request.RequestUri.DnsSafeHost;
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Bearer realm=\"{0}\"", host));
                return;
            }

            // Get bot ID from MSA app ID
            Claim botClaim = identity.Claims.FirstOrDefault(c => c.Issuer == Issuer && c.Type == "aud");
            if (botClaim == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
                return;
            }

            var principal = new ClaimsPrincipal(identity);

            Thread.CurrentPrincipal = principal;

            // inside of ASP.NET this is required
            if (HttpContext.Current != null)
                HttpContext.Current.User = principal;

            await base.OnAuthorizationAsync(actionContext, cancellationToken);
        }
    }
}