using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Bot.Sample.GraphBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.GraphBot
{
    // https://github.com/aspnet/Security/tree/8b4b99b168c97b6c559ec0f971ae6389a669c153/samples/OpenIdConnect.AzureAdSample
    public class Startup
    {
        private readonly IConfiguration configuration;
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            this.configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.AddMvc();
            services.AddSession();

            services.AddSingleton<IConfiguration>(this.configuration);
            services.AddSingleton<IClientKeys, ClientKeys>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug(Extensions.Logging.LogLevel.Trace);
                app.UseDeveloperExceptionPage();
                app.UseRuntimeInfoPage();
            }
            else
            {

            }

            app.UseSession();

            ConfigureAuth(app);

            app.Run(async (context) =>
            {
                await Task.Yield();
            });
        }

        public void ConfigureAuth(IApplicationBuilder app)
        {
            IClientKeys keys = new ClientKeys(this.configuration);

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            { 
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,

                CookieSecure = CookieSecureOption.SameAsRequest,
            });

            const string Authority = "https://login.microsoftonline.com/common/";

            app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,

                ClientId = keys.ClientID,
                ClientSecret = keys.ClientSecret,
                Authority = Authority,

                ResponseType = OpenIdConnectResponseTypes.CodeIdToken,
                SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme,
                TokenValidationParameters = new TokenValidationParameters
                {
                    // I believe this is used in multi-tenant applications, if you want to validate a tenant's access to your application
                    ValidateIssuer = false
                },

                Events = new OpenIdConnectEvents()
                {
                    OnAuthorizationCodeReceived = async context =>
                    {
                        // given the authorization code
                        var authorizationCode = context.ProtocolMessage.Code;
                        var request = context.HttpContext.Request;
                        var redirectUri = new Uri(UriHelper.Encode(request.Scheme, request.Host, request.PathBase, request.Path));

                        // get and verify the access token and refresh token
                        var credential = new ClientCredential(keys.ClientID, keys.ClientSecret);
                        var tokenCache = new TokenCache();
                        var authContext = new AuthenticationContext(Authority, tokenCache);
                        var result = await authContext.AcquireTokenByAuthorizationCodeAsync(authorizationCode, redirectUri, credential, Keys.Resource);

                        // serialize the per-user TokenCache
                        var tokenBlob = tokenCache.Serialize();

                        // and store it in the authentication properties so that the Controller can access it
                        context.Properties.Items.Add(Keys.TokenCache, Convert.ToBase64String(tokenBlob));

                        context.HandleCodeRedemption(result.AccessToken, result.IdToken);
                    }
                }
            });

            app.UseMvc();
        }
    }
}
