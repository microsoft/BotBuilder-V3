using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Logging;
using Activity = Microsoft.Bot.Connector.Activity;

namespace Microsoft.Bot.Sample.AspNetCore.SimpleIVRBot.Controllers
{

    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            await context.PostAsync("You said: " + message.Text);
            context.Wait(MessageReceivedAsync);
        }
    }
    
    [BotAuthentication]
    [Route("api/[controller]")]
    public class MessagesController : Controller
    {

        private readonly ILogger<MessagesController> logger;

        public MessagesController(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<MessagesController>();
        }

        // POST api/values
        [HttpPost]
        public virtual async Task<IActionResult> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, () => new EchoDialog());
                        break;

                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        Trace.TraceError($"Unknown activity type ignored: {activity.GetActivityType()}");
                        break;
                }
            }
            return StatusCode((int) HttpStatusCode.Accepted);
        }
    }
}
