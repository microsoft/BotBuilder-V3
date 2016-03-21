using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Form;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.AnnotatedSandwichBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static IForm<SandwichOrder> sandwichForm = new Form<SandwichOrder>("SandwichForm")
            .Message("Welcome to the simple sandwich order bot!")
            .AddRemainingFields()
            .Confirm("Do you want to change anything in your {Length} {Sandwich} on {Bread} {&Bread} with {[Cheese Toppings Sauces]}?")
            ;

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                // This adds the SandwichOrder form to the dialog and sets it up as the default dialog
                var dialogs = new DialogCollection().Add(sandwichForm).Add(OrderFlowDialog.Instance);
                return await ConnectorSession.MessageReceivedAsync(Request, message, dialogs, OrderFlowDialog.Instance);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
                // return HandleSystemMessage(message);
            }
        }

        private sealed class OrderFlowDialog : IDialog
        {
            public static readonly IDialog Instance = new OrderFlowDialog();

            string IDialog.ID
            {
                get
                {
                    return typeof(OrderFlowDialog).Name;
                }
            }

            Task<Message> IDialog.BeginAsync(ISession session, Task<object> taskArguments)
            {
                return session.BeginDialogAsync(sandwichForm, Tasks.Null);
            }

            Task<Message> IDialog.DialogResumedAsync(ISession session, Task<object> taskResult)
            {
                return session.CreateDialogResponse("sandwich being made");
            }

            Task<Message> IDialog.ReplyReceivedAsync(ISession session)
            {
                return session.CreateDialogResponse("sandwich out for delivery");
            }
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}