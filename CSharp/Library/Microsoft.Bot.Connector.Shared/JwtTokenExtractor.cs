using System;
using System.Collections.Generic;
#if NET45
using System.Diagnostics;
#endif
#if NET45
using System.IdentityModel.Tokens;
#else
using System.IdentityModel.Tokens.Jwt;
#endif
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Protocols;
#if !NET45
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
#endif

namespace Microsoft.Bot.Connector
{
    public class JwtTokenExtractor
    {
        /// <summary>
        /// Shared of OpenIdConnect configuration managers (one per metadata URL)
        /// </summary>
        private static readonly Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _openIdMetadataCache =
            new Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();

        /// <summary>
        /// Token validation parameters for this instance
        /// </summary>
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// OpenIdConnect configuration manager for this instances
        /// </summary>
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _openIdMetadata;

        public JwtTokenExtractor(TokenValidationParameters tokenValidationParameters, string metadataUrl)
        {
            // Make our own copy so we can edit it
            _tokenValidationParameters = tokenValidationParameters.Clone();
            _tokenValidationParameters.RequireSignedTokens = true;

            if (!_openIdMetadataCache.ContainsKey(metadataUrl))
#if NET45
                _openIdMetadataCache[metadataUrl] = new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl);
#else
                _openIdMetadataCache[metadataUrl] = new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl, new OpenIdConnectConfigurationRetriever());
#endif

            _openIdMetadata = _openIdMetadataCache[metadataUrl];
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(HttpRequestMessage request)
        {
            if (request.Headers.Authorization != null)
                return await GetIdentityAsync(request.Headers.Authorization.Scheme, request.Headers.Authorization.Parameter).ConfigureAwait(false);
            return null;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string authorizationHeader)
        {
            if (authorizationHeader == null)
                return null;

            string[] parts = authorizationHeader?.Split(' ');
            if (parts.Length == 2)
                return await GetIdentityAsync(parts[0], parts[1]).ConfigureAwait(false);
            return null;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string scheme, string parameter)
        {
            // No header in correct scheme or no token
            if (scheme != "Bearer" || string.IsNullOrEmpty(parameter))
                return null;

            // Issuer isn't allowed? No need to check signature
            if (!HasAllowedIssuer(parameter))
                return null;

            try
            {
                ClaimsPrincipal claimsPrincipal = await ValidateTokenAsync(parameter).ConfigureAwait(false);
                return claimsPrincipal.Identities.OfType<ClaimsIdentity>().FirstOrDefault();
            }
            catch (Exception e)
            {
#if NET45
                Trace.TraceWarning("Invalid token. " + e.ToString());
#else
                IdentityModel.Logging.LogHelper.LogException<Exception>($"Invalid token. {e.ToString()}");
#endif
                return null;
            }
        }

        private bool HasAllowedIssuer(string jwtToken)
        {
            JwtSecurityToken token = new JwtSecurityToken(jwtToken);
            if (_tokenValidationParameters.ValidIssuer != null && _tokenValidationParameters.ValidIssuer == token.Issuer)
                return true;

            if ((_tokenValidationParameters.ValidIssuers ?? Enumerable.Empty<string>()).Contains(token.Issuer))
                return true;

            return false;
        }



        public string GetAppIdFromClaimsIdentity(ClaimsIdentity identity)
        {
            if (identity == null)
                return null;

            Claim botClaim = identity.Claims.FirstOrDefault(c => _tokenValidationParameters.ValidIssuers.Contains(c.Issuer) && c.Type == "aud");
            return botClaim?.Value;
        }

        public string GetAppIdFromEmulatorClaimsIdentity(ClaimsIdentity identity)
        {
            if (identity == null)
                return null;

            Claim appIdClaim = identity.Claims.FirstOrDefault(c => _tokenValidationParameters.ValidIssuers.Contains(c.Issuer) && c.Type == "appid");
            if (appIdClaim == null)
                return null;

            // v3.1 emulator token
            if (identity.Claims.Any(c => c.Type == "aud" && c.Value == appIdClaim.Value))
                return appIdClaim.Value;

            // v3.0 emulator token -- allow this
            if (identity.Claims.Any(c => c.Type == "aud" && c.Value == "https://graph.microsoft.com"))
                return appIdClaim.Value;

            return null;
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
#if NET45
                Trace.TraceError($"Error refreshing OpenId configuration: {e}");
#else
                IdentityModel.Logging.LogHelper.LogException<Exception>($"Error refreshing OpenId configuration: {e}");
#endif

                // No config? We can't continue
                if (config == null)
                    throw;
            }

            // Update the signing tokens from the last refresh
#if NET45
            _tokenValidationParameters.IssuerSigningTokens = config.SigningTokens;
#else
            _tokenValidationParameters.IssuerSigningKeys = config.SigningKeys;
#endif

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                SecurityToken parsedToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(jwtToken, _tokenValidationParameters, out parsedToken);
                return principal;
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
#if NET45
                string keys = string.Join(", ", ((config?.SigningTokens) ?? Enumerable.Empty<SecurityToken>()).Select(t => t.Id));
                Trace.TraceError("Error finding key for token. Available keys: " + keys);
#else
                string keys = string.Join(", ", ((config?.SigningKeys) ?? Enumerable.Empty<SecurityKey>()).Select(t => t.KeyId));
                IdentityModel.Logging.LogHelper.LogException<SecurityTokenSignatureKeyNotFoundException>("Error finding key for token.Available keys: " + keys);
#endif
                throw;
            }
        }
    }
}