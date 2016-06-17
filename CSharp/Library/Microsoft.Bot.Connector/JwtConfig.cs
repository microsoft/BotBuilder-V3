using System;
using System.IdentityModel.Tokens;
using System.Linq;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Configuration for JWT tokens
    /// </summary>
    public static class JwtConfig
    {
        /// <summary>
        /// TO BOT FROM CHANNEL: OpenID metadata document for tokens coming from MSA
        /// </summary>
        public const string ToBotFromChannelOpenIdMetadataUrl = "https://api.aps.skype.com/v1/.well-known/openidconfiguration";

        /// <summary>
        /// TO BOT FROM CHANNEL: Token validation parameters when connecting to a bot
        /// </summary>
        public static TokenValidationParameters GetToBotFromChannelTokenValidationParameters(string msaAppId)
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://api.botframework.com" },
                ValidateAudience = true,
                ValidAudiences = new[] { msaAppId },
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true
            };
        }

        /// <summary>
        /// TO BOT FROM MSA: OpenID metadata document for tokens coming from MSA
        /// </summary>
        /// <remarks>
        /// These settings are used to allow access from the Bot Framework Emulator
        /// </remarks>
        public const string ToBotFromMSAOpenIdMetadataUrl = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

        /// <summary>
        /// TO BOT FROM MSA: Token validation parameters when connecting to a channel
        /// </summary>
        /// <remarks>
        /// These settings are used to allow access from the Bot Framework Emulator
        /// </remarks>
        public static readonly TokenValidationParameters ToBotFromMSATokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://sts.windows.net/72f988bf-86f1-41af-91ab-2d7cd011db47/" },
            ValidateAudience = true,
            ValidAudiences = new[] { "https://graph.microsoft.com" },
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireSignedTokens = true
        };
    }
}
