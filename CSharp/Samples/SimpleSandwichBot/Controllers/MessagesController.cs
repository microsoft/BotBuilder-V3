using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Form;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.SimpleSandwichBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private static IForm<SandwichOrder> sandwichForm = new Form<SandwichOrder>("SandwichForm")
            .Message("Welcome to the simple sandwich order bot!")
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
                var dialogs = new DialogCollection().Add(sandwichForm);
                return await ConnectorSession.MessageReceivedAsync(Request, message, dialogs, sandwichForm);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
                // return HandleSystemMessage(message);
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