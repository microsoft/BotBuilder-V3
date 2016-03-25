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
        private static IForm<SandwichOrder> MakeForm()
        {
            return FormBuilder<SandwichOrder>
                        .Start()
                        .Message("Welcome to the simple sandwich order bot!")
                        .AddRemainingFields()
                        .Confirm("Do you want to order your {Length} {Sandwich} on {Bread} {&Bread} with {[{Cheese} {Toppings} {Sauces}]}?")
                        .Build();
        }

        internal static IFormDialog<SandwichOrder> MakeRoot()
        {
            return new FormDialog<SandwichOrder>(MakeForm);
        }

        /// <summary>
        /// POST: api/Messages
        /// receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                return await CompositionRoot.SendAsync(message, MakeRoot);
            }
            else
            {
                return HandleSystemMessage(message);
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