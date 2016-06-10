using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Autofac;
using Microsoft.Bot.Sample.GraphBot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using System.Net.Http;
using System.Diagnostics;

namespace Microsoft.Bot.Sample.GraphBot.Controllers
{
    [Route("api/[controller]")]
    //[BotAuthentication(Keys.Bot.ID, Keys.Bot.Secret)]
    public class MessagesController : Controller
    {
        private readonly IClientKeys keys;
        public MessagesController(IClientKeys keys)
        {
            SetField.NotNull(out this.keys, nameof(keys), keys);
        }

        [HttpPost]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity toBot, CancellationToken token)
        {
            if (toBot != null)
            {
                // one of these will have an interface and process it
                switch (toBot.GetActivityType())
                {
                    case ActivityTypes.Message:
                        using (var scope = DialogModule.BeginLifetimeScope(Container.Instance, toBot))
                        {
                            // construct the authentication url
                            var builder = new UriBuilder(this.Request.GetEncodedUrl());
                            builder.Path = $"/api/account/";
                            var uri = builder.Uri;

                            // make the authentication url and the client keys available to root dialog if needed
                            // somewhat hacky - would be better done by uniting Autofac and ASP.NET DI containers
                            scope.Resolve<Uri>(TypedParameter.From(uri));
                            scope.Resolve<IClientKeys>(TypedParameter.From(this.keys));

                            // post the message to the bot
                            var task = scope.Resolve<IPostToBot>();
                            await task.PostAsync(toBot, token);

                        }
                        break;

                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {toBot.GetActivityType()}");
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}
