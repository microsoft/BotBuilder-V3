using System.Web.Http;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Web.Http.Description;
using System.Diagnostics;

namespace Microsoft.Bot.Sample.AnnotatedSandwichBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
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
        [ResponseType(typeof(void))]
        public virtual async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            if (activity != null)
            {
                // one of these will have an interface and process it
                switch (activity.GetActivityType())
                {
                    case ActivityTypes.Message:
                        await Conversation.SendAsync(activity, MakeJsonRootDialog);
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
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }
    }
}