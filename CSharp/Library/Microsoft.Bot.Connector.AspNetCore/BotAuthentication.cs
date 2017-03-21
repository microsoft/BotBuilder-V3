using System;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Extension methods to add BotAuthentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class BotAuthenticationAppBuilderExtensions
    {
        public static IApplicationBuilder UseBotAuthentication(this IApplicationBuilder app, string microsoftAppId, string microsoftAppPassword)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseBotAuthentication(new StaticCredentialProvider(microsoftAppId, microsoftAppPassword));
        }

        public static IApplicationBuilder UseBotAuthentication(this IApplicationBuilder app, ICredentialProvider credentialProvider)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var options = new BotAuthenticationOptions
            {
                CredentialProvider = credentialProvider
            };

            return app.UseMiddleware<BotAuthenticationMiddleware>(Options.Create(options));
        }
        
        public static IApplicationBuilder UseBotAuthentication(this IApplicationBuilder app, BotAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<BotAuthenticationMiddleware>(Options.Create(options));
        }
    }

    
    /// <summary>
    /// Bot authentication middleware. <see cref="AuthenticationMiddleware{TOptions}"/> for more information.
    /// </summary>
    public sealed class BotAuthenticationMiddleware : AuthenticationMiddleware<BotAuthenticationOptions>
    {
        public BotAuthenticationMiddleware(RequestDelegate next, IOptions<BotAuthenticationOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder)
            : base(next, options, loggerFactory, encoder)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
        }

        protected override AuthenticationHandler<BotAuthenticationOptions> CreateHandler()
        {
            return new BotAuthenticationHandler();
        }
    }
}
