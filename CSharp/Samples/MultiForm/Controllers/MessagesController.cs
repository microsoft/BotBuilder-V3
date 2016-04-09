using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Bot.Sample.MultiForm
{
    // [BotAuthentication]
    public class MessagesController : ApiController
    {
        [Serializable]
        public class MyBot : IDialog
        {
            async Task IDialog<object>.StartAsync(IDialogContext context)
            {
                context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice()), WhatDoYouWant);
            }

            public async Task WhatDoYouWant(IDialogContext context, IAwaitable<TopChoice> choices)
            {
                switch ((await choices).Choice.Value)
                {
                    case TopChoices.Joke:
                        context.Call<ChooseJoke>(new FormDialog<ChooseJoke>(new ChooseJoke(), options: FormOptions.PromptInStart),
                            TellAJoke);
                        break;
                    default:
                        await context.PostAsync("I don't understand");
                        context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice(), options: FormOptions.PromptInStart), WhatDoYouWant);
                        break;
                }
            }

            public async Task TellAJoke(IDialogContext context, IAwaitable<ChooseJoke> joke)
            {
                switch ((await joke).KindOfJoke)
                {
                    case TypeOfJoke.Funny:
                        await context.PostAsync("Something funny");
                        break;
                    case TypeOfJoke.KnockKnock:
                        await context.PostAsync("Knock-knock...");
                        break;
                }
                context.Done<object>(null);
                // context.Call<TopChoice>(new FormDialog<TopChoice>(new TopChoice(), options: FormOptions.PromptInStart), WhatDoYouWant);
            }
        }

        public enum TopChoices { Joke, Weather }

        [Serializable]
        public class TopChoice
        {
            public TopChoices? Choice;
        }

        public enum TypeOfJoke { Funny, KnockKnock };

        [Serializable]
        public class ChooseJoke
        {
            public TypeOfJoke? KindOfJoke;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                // return our reply to the user
                return await Conversation.SendAsync(message, () => new MyBot());
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
                // Implement user deletion here
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