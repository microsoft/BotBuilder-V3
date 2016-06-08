using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace Microsoft.Bot.Connector
{
    public class JwtTokenExtractor
    {
        private readonly string[] _allowedAudiences;
        private readonly string[] _allowedIssuers;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _openIdMetadata;

        public JwtTokenExtractor(
            IEnumerable<string> allowedAudiences,
            IEnumerable<string> allowedIssuers,
            string metadataUrl)
        {
            _allowedAudiences = allowedAudiences.ToArray();
            _allowedIssuers = allowedIssuers.ToArray();
            _openIdMetadata = new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl);
        }

        public async Task<JwtIdentity> GetIdentityAsync(HttpRequestMessage request)
        {
            if (request?.Headers?.Authorization?.Scheme != "Bearer")
            {
                // No auth header with Bearer scheme
                return null;
            }

            string jwtToken = request.Headers.Authorization.Parameter;

            try
            {
                ClaimsPrincipal claimsPrincipal = await ValidateTokenAsync(jwtToken).ConfigureAwait(false);
                return new JwtIdentity(jwtToken, claimsPrincipal.Identities.OfType<ClaimsIdentity>().FirstOrDefault());
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Invalid token. " + e.ToString());
                return null;
            }
        }

        private async Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken)
        {
            // _openIdMetadata only does a full refresh when the cache expires every 5 days
            OpenIdConnectConfiguration config = null;
            try
            {
                 config = await _openIdMetadata.GetConfigurationAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error refreshing OpenId configuration: {e}");

                // No config? We can't continue
                if (config == null)
                    throw;
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters = new TokenValidationParameters();
            validationParameters.ValidateAudience = true;
            validationParameters.ValidAudiences = _allowedAudiences;
            validationParameters.ValidateIssuer = true;
            validationParameters.ValidIssuers = _allowedIssuers;
            validationParameters.ValidateLifetime = true;
            validationParameters.ClockSkew = TimeSpan.FromMinutes(5);
            validationParameters.RequireSignedTokens = true;
            validationParameters.IssuerSigningTokens = config.SigningTokens;

            SecurityToken parsedToken;
            ClaimsPrincipal principal = tokenHandler.ValidateToken(jwtToken, validationParameters, out parsedToken);
            return principal;
        }
    }
}
