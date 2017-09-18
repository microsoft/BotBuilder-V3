using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

namespace Microsoft.Bot.Connector
{

    partial class BotAuthenticator
    {

        /// <summary>
        /// Generates <see cref="HttpStatusCode.Unauthorized"/> response for the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="reason">The reason phrase for unauthorized status code.</param>
        /// <returns>A response with status code unauthorized.</returns>
        public static IActionResult GenerateUnauthorizedResponse(HttpContext context, string reason = "")
        {
            var host = context.Request.Host.Value;
            context.Response.Headers.Add("WWW-Authenticate", string.Format("Bearer realm=\"{0}\"", host));
            if (!string.IsNullOrEmpty(reason))
            {
                return new ContentResult
                {
                    Content = reason,
                    ContentType = "text/plain; charset=utf-8",
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }
            return new UnauthorizedResult();
        }

        /// <summary>
        /// Authenticates the incoming request and add the <see cref="IActivity.ServiceUrl"/> for each
        /// activities to <see cref="MicrosoftAppCredentials.TrustedHostNames"/> if the request is authenticated.
        /// </summary>
        /// <param name="request"> The request that should be authenticated.</param>
        /// <param name="activities"> The activities extracted from request.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns></returns>
        public async Task<bool> TryAuthenticateAsync(HttpRequest request, IEnumerable<IActivity> activities,
            CancellationToken token)
        {
            var authorizationHeader = request.Headers.ContainsKey(HeaderNames.Authorization)
                ? AuthenticationHeaderValue.Parse(request.Headers[HeaderNames.Authorization])
                : null;
            var identityToken = await this.TryAuthenticateAsyncWithActivity(authorizationHeader, activities, token);
            identityToken.ValidateServiceUrlClaim(activities);
            TrustServiceUrls(identityToken, activities);
            return identityToken.Authenticated;
        }

        /// <summary>
        /// Authenticates the request and returns the IdentityToken.
        /// </summary>
        /// <param name="request"> The request that should be authenticated.</param>
        /// <param name="activities"> The activities extracted from request.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns> The <see cref="IdentityToken"/>.</returns>
        public async Task<IdentityToken> AuthenticateAsync(HttpRequest request, IEnumerable<IActivity> activities,
            CancellationToken token)
        {
            var authorizationHeader = request.Headers.ContainsKey(HeaderNames.Authorization)
                ? AuthenticationHeaderValue.Parse(request.Headers[HeaderNames.Authorization])
                : null;
            var identityToken = await this.TryAuthenticateAsyncWithActivity(authorizationHeader, activities, token);
            identityToken.ValidateServiceUrlClaim(activities);
            TrustServiceUrls(identityToken, activities);
            return identityToken;
        }
        
    }
}
