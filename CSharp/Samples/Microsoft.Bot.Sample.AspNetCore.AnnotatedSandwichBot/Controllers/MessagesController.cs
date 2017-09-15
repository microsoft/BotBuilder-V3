using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Sample.AspNetCore.AnnotatedSandwichBot.Controllers
{
    [Route("api/[controller]")]
    [BotAuthentication]
    public class MessagesController : Controller
    {

        private readonly ILogger logger;

        public MessagesController(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            this.logger = loggerFactory.CreateLogger<MessagesController>();
        }

        internal static IDialog<SandwichOrder> MakeRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(SandwichOrder.BuildLocalizedForm))
                .Do(async (context, order) =>
                {
                    try
                    {
                        var completed = await order;
                        // Actually process the sandwich order...
                        await context.PostAsync("Processed your order!");
                    }
                    catch (FormCanceledException<SandwichOrder> e)
                    {
                        string reply;
                        if (e.InnerException == null)
                        {
                            reply = $"You quit on {e.Last}--maybe you can finish next time!";
                        }
                        else
                        {
                            reply = "Sorry, I've had a short circuit.  Please try again.";
                        }
                        await context.PostAsync(reply);
                    }
                });
        }

        internal static IDialog<JObject> MakeJsonRootDialog()
        {
            return Chain.From(() => FormDialog.FromForm(SandwichOrder.BuildJsonForm))
                .Do(async (context, order) =>
                {
                    try
                    {
                        var completed = await order;
                        // Actually process the sandwich order...
                        await context.PostAsync("Processed your order!");
                    }
                    catch (FormCanceledException<JObject> e)
                    {
                        string reply;
                        if (e.InnerException == null)
                        {
                            reply = $"You quit on {e.Last}--maybe you can finish next time!";
                        }
                        else
                        {
                            reply = "Sorry, I've had a short circuit.  Please try again.";
                        }
                        await context.PostAsync(reply);
                    }
                });
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and send replies
        /// </summary>
        /// <param name="activity"></param>
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, MakeRootDialog);
                        break;

                    case ActivityTypes.ConversationUpdate:
                    case ActivityTypes.ContactRelationUpdate:
                    case ActivityTypes.Typing:
                    case ActivityTypes.DeleteUserData:
                    default:
                        logger.LogWarning("Unknown activity type ignored: {0}", activity.GetActivityType());
                        break;
                }
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}