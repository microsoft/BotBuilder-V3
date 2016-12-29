using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Options for <see cref="BotAuthenticationMiddleware"/>.
    /// </summary>
    public sealed class BotAuthenticationOptions : AuthenticationOptions
    {
        public BotAuthenticationOptions() : base()
        {
            AuthenticationScheme = JwtBearerDefaults.AuthenticationScheme;
            AutomaticAuthenticate = true;
            AutomaticChallenge = false;
        }

        /// <summary>
        /// The <see cref="ICredentialProvider"/> used for authentication.
        /// </summary>
        public ICredentialProvider CredentialProvider { set; get; }

        /// <summary>
        /// The OpenId configuation.
        /// </summary>
        public string OpenIdConfiguration { set; get; } = JwtConfig.ToBotFromChannelOpenIdMetadataUrl;

        /// <summary>
        /// Flag indicating if emulator tokens should be disabled.
        /// </summary>
        public bool DisableEmulatorTokens { set; get; } = false;

        /// <summary>
        /// Flag indicating if <see cref="BotAuthenticationHandler"/> should be stored in 
        /// the returned <see cref="Microsoft.AspNetCore.Authentication.AuthenticationTicket"/>. 
        /// </summary>
        public bool SaveToken { set; get; } = true;
    }
}
