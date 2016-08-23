using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Bot.Sample.AlarmBot.Controllers
{
    // Autofac provides a mechanism to inject ActionFilterAttributes from the container
    // but seems to require the implementation of special interfaces
    // https://github.com/autofac/Autofac.WebApi/issues/6
    [BotAuthentication]
    public sealed class MessagesController : ApiController
    {
        // TODO: "service locator"
        private readonly ILifetimeScope scope;
        public MessagesController(ILifetimeScope scope)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
        }
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity, CancellationToken token)
        {
            if (activity != null)
            {
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        using (var scope = DialogModule.BeginLifetimeScope(this.scope, activity))
                        {
                            var postToBot = scope.Resolve<IPostToBot>();
                            await postToBot.PostAsync(activity, token);
                        }

                        break;
                }
            }

            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}