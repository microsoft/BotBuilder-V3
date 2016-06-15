using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Microsoft.Bot.Sample.GraphBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Microsoft.Bot.Sample.GraphBot.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : Controller
    {
        [HttpGet("{cookie}")]
        public async Task<string> Get(string cookie, CancellationToken token)
        {
            // get the TokenCache stored per-user from within OpenIdConnectOptions.Events.OnAuthorizationCodeReceived
            var authenticateContext = new AuthenticateContext(CookieAuthenticationDefaults.AuthenticationScheme);
            await this.HttpContext.Authentication.AuthenticateAsync(authenticateContext);
            string tokenBase64;
            if (authenticateContext.Properties.TryGetValue(Keys.TokenCache, out tokenBase64))
            {
                byte[] tokenBlob = Convert.FromBase64String(tokenBase64);

                // decode the resumption cookie from the url
                var resume = UrlToken.Decode<ResumptionCookie>(cookie);
                var continuation = resume.GetMessage();
                using (var scope = DialogModule.BeginLifetimeScope(Container.Instance, continuation))
                {
                    var botData = scope.Resolve<IBotData>();
                    await botData.LoadAsync(token);
                    var data = botData.UserData; 

                    var tenantID = this.User.FindFirst(Keys.TenantID);
                    var objectIdentifier = this.User.FindFirst(Keys.ObjectID);

                    data.SetValue(Keys.ObjectID, objectIdentifier.Value);
                    data.SetValue(Keys.TenantID, tenantID.Value);
                    data.SetValue(Keys.TokenCache, tokenBlob);

                    await botData.FlushAsync(token); 
                }

                return "You're now logged-in - continue talking to the bot!";
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
