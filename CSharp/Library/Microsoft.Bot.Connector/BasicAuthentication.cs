using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// This attribute enforces that a caller uses Basic Auth passing your AppId and AppSecret
    /// </summary>
    /// <remarks>
    /// NOTE: You must use SSL for this to be secure, as it uses BasicAtuh
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class BasicAuthentication : AuthorizationFilterAttribute
    {
        private static Lazy<string> _appId = new Lazy<string>(() => ConfigurationManager.AppSettings["AppId"]);
        private static Lazy<string> _appSecret = new Lazy<string>(() => ConfigurationManager.AppSettings["AppSecret"]);

        private string appId;
        private string appSecret;

        public BasicAuthentication()
        {
        }

        public BasicAuthentication(string appId, string appSecret)
        {
            this.appId = appId ?? _appId.Value;
            this.appSecret = appSecret ?? _appSecret.Value;

            if (this.appId == null)
                throw new ArgumentNullException("Missing AppId");

            if (this.appSecret == null)
                throw new ArgumentNullException("Missing AppSecret");
        }

        /// <summary>
        /// Override to Web API filter method to handle Basic Auth check
        /// </summary>
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext.Request.RequestUri.Host == "localhost")
                return;

            var identity = await ParseAuthorizationHeaderAsync(actionContext);
            if (identity == null)
            {
                var host = actionContext.Request.RequestUri.DnsSafeHost;
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Basic realm=\"{0}\"", host));
                return;
            }

            if (await OnAuthorizeUser(identity, actionContext))
            {
                var principal = new ClaimsPrincipal(identity);

                Thread.CurrentPrincipal = principal;

                // inside of ASP.NET this is required
                if (HttpContext.Current != null)
                    HttpContext.Current.User = principal;

                await base.OnAuthorizationAsync(actionContext, cancellationToken);
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
                return;
            }
        }

        /// <summary>
        /// Base implementation for user authentication - you probably will
        /// want to override this method for application specific logic.
        /// 
        /// The base implementation merely checks for username and password
        /// present and set the Thread principal.
        /// 
        /// Override this method if you want to customize Authentication
        /// and store user data as needed in a Thread Principle or other
        /// Request specific storage.
        /// </summary>
#pragma warning disable 1998 // Disable "not awaiting in this method" warning
        protected virtual async Task<bool> OnAuthorizeUser(BasicAuthIdentity identity, HttpActionContext actionContext)
        {
            if (actionContext.Request.RequestUri.Host == "localhost")
                return true;

            if (this.appId == null)
                throw new ArgumentNullException("Missing AppId");

            if (this.appSecret == null)
                throw new ArgumentNullException("Missing AppSecret");

            if (identity?.Id == this.appId && identity?.Password == this.appSecret)
                return true;

            return false;
        }
#pragma warning restore 1998

        /// <summary>
        /// Parses the Authorization header and creates user credentials
        /// </summary>
        protected virtual Task<BasicAuthIdentity> ParseAuthorizationHeaderAsync(HttpActionContext actionContext)
        {
            string authHeader = null;
            var auth = actionContext.Request?.Headers?.Authorization;
            if (auth != null && auth.Scheme == "Basic")
                authHeader = auth.Parameter;

            if (!string.IsNullOrEmpty(authHeader))
            {
                authHeader = Encoding.Default.GetString(Convert.FromBase64String(authHeader));

                string[] parts = authHeader.Split(':');
                if (parts.Length == 2)
                {
                    string id = parts[0].Trim();
                    string password = parts[1].Trim();
                    return Task.FromResult(new BasicAuthIdentity(id, password));
                }
            }
            return Task.FromResult<BasicAuthIdentity>(null);
        }
    }
}
