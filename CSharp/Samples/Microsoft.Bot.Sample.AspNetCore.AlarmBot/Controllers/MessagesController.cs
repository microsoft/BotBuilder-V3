using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Autofac;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Sample.AspNetCore.AlarmBot.Controllers
{
    // Autofac provides a mechanism to inject ActionFilterAttributes from the container
    // but seems to require the implementation of special interfaces
    // https://github.com/autofac/Autofac.WebApi/issues/6
    [Route("api/[controller]")]
    [BotAuthentication]
    public sealed class MessagesController : Controller
    {
        // TODO: "service locator"
        private readonly ILifetimeScope scope;

        public MessagesController(ILifetimeScope scope)
        {
            SetField.NotNull(out this.scope, nameof(scope), scope);
        }

        // POST api/values
        [HttpPost]
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