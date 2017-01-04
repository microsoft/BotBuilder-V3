using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Bot authentication hanlder used by <see cref="BotAuthenticationMiddleware"/>.
    /// </summary>
    public sealed class BotAuthenticationHandler : AuthenticationHandler<BotAuthenticationOptions>
    {
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (await Options.CredentialProvider.IsAuthenticationDisabledAsync())
            {
                var principal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Bot") }));
                return AuthenticateResult.Success(new AuthenticationTicket(principal, new AuthenticationProperties(), Options.AuthenticationScheme));
            }

            string token = null;

            string authorization = Request.Headers["Authorization"];
            token = authorization?.Substring("Bearer ".Length).Trim();

            // If no token found, no further work possible
            // and Authentication is not disabled fail
            if (string.IsNullOrEmpty(token))
            {
                return AuthenticateResult.Fail("No JwtToken is present and BotAuthentication is enabled!");
            }

            var authenticator = new BotAuthenticator(Options.CredentialProvider, Options.OpenIdConfiguration, Options.DisableEmulatorTokens);
            var identityToken = await authenticator.TryAuthenticateAsync(Options.AuthenticationScheme, token, CancellationToken.None);

            if (identityToken.Authenticated)
            {
                identityToken.Identity.AddClaim(new Claim(ClaimTypes.Role, "Bot"));
                var principal = new ClaimsPrincipal(identityToken.Identity);
                var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), Options.AuthenticationScheme);
                Context.User = principal;

                if (Options.SaveToken)
                {
                    ticket.Properties.StoreTokens(new[]
                            {
                                new AuthenticationToken { Name = "access_token", Value = token }
                            });
                }

                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail($"Failed to authenticate JwtToken {token}");
            }
        }
    }
}
