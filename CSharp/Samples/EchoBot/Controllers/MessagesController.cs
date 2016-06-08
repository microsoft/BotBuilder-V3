using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Web.Http.Description;
using System.Diagnostics;

namespace Microsoft.Bot.Sample.EchoBot
{
    //[BotAuthentication]
    public class MessagesController : ApiController
    {
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
                        //return await DirectConversation.SendDirectAsync(activity, () => new EchoDialog());
                        //return await Conversation.SendAsync(activity, () => new EchoDialog());
                        //return await Conversation.SendAsync(activity, () => EchoCommandDialog.dialog);
                        //return await Conversation.SendAsync(activity, () => new EchoAttachmentDialog());
                        await Conversation.SendAsync(activity, () => EchoChainDialog.dialog);
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
