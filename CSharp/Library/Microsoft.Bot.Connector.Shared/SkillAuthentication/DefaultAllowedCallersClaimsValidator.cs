// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Microsoft.Bot.Connector.SkillAuthentication
{
    /// <summary>
    /// Claims validator which checks that requests are coming only from allowed parent bots.
    /// </summary>
    public class DefaultAllowedCallersClaimsValidator : ClaimsValidator
    {
        private readonly IList<string> _allowedCallers;

        public DefaultAllowedCallersClaimsValidator(IList<string> allowedCallers)
        {
            // To allow all Parent bots, AllowedCallers should have one element of '*'

            _allowedCallers = allowedCallers ?? throw new ArgumentNullException(nameof(allowedCallers));
            if (!_allowedCallers.Any())
            {
                throw new ArgumentNullException(nameof(allowedCallers), "AllowedCallers must contain at least one element of '*' or valid MicrosoftAppId(s).");
            }
        }

        public override Task ValidateClaimsAsync(IList<Claim> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            if (!claims.Any())
            {
                throw new UnauthorizedAccessException("ValidateClaimsAsync.claims parameter must contain at least one element.");
            }

            if (SkillValidation.IsSkillClaim(claims))
            {
                // if _allowedCallers has one item of '*', allow all parent bot calls and do not validate the appid from claims
                if (_allowedCallers.Count == 1 && _allowedCallers[0] == "*")
                {
                    return Task.CompletedTask;
                }

                // Check that the appId claim in the skill request is in the list of skills configured for this bot.
                var appId = JwtTokenValidation.GetAppIdFromClaims(claims).ToUpperInvariant();
                if (_allowedCallers.Contains(appId))
                {
                    return Task.CompletedTask;
                }
                
                throw new UnauthorizedAccessException($"Received a request from a bot with an app ID of \"{appId}\". To enable requests from this caller, add the app ID to your configuration file.");
            }

            throw new UnauthorizedAccessException($"ValidateClaimsAsync called without a Skill claim in claims.");
        }
    }
}