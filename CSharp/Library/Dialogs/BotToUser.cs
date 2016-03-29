using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class ReactiveBotToUser : IBotToUser
    {
        private readonly Message toBot;
        private readonly IConnectorClient client;
        public ReactiveBotToUser(Message toBot, IConnectorClient client)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
            SetField.NotNull(out this.client, nameof(client), client);
        }

        public Message ToUser
        {
            get
            {
                if (this.toUser != null)
                {
                    return this.toUser;
                }
                else
                {
                    IBotToUser botToUser = this;
                    return botToUser.MakeMessage();
                }
            }
        }

        Message IBotToUser.MakeMessage()
        {
            var toUser = this.toBot.CreateReplyMessage();
            toUser.BotUserData = this.toBot.BotUserData;
            toUser.BotConversationData = this.toBot.BotConversationData;
            toUser.BotPerUserInConversationData = this.toBot.BotPerUserInConversationData;
            return toUser;
        }

        private Message toUser;

        async Task IBotToUser.PostAsync(Message message, CancellationToken cancellationToken)
        {
            if (this.toUser != null)
            {
                await this.client.Messages.SendMessageAsync(this.toUser, cancellationToken);
                this.toUser = null;
            }

            SetField.NotNull(out this.toUser, nameof(message), message);
        }
    }

    public sealed class BotToUserQueue : IBotToUser
    {
        private readonly Message toBot;
        private readonly Queue<Message> queue = new Queue<Message>();
        public BotToUserQueue(Message toBot)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
        }
       
        public void Clear()
        {
            this.queue.Clear();
        }

        public IEnumerable<Message> Messages
        {
            get
            {
                return this.queue;
            }
        }

        Message IBotToUser.MakeMessage()
        {
            var toUser = this.toBot.CreateReplyMessage();
            toUser.BotUserData = this.toBot.BotUserData;
            toUser.BotConversationData = this.toBot.BotConversationData;
            toUser.BotPerUserInConversationData = this.toBot.BotPerUserInConversationData;
            return toUser;
        }

        async Task IBotToUser.PostAsync(Message message, CancellationToken cancellationToken)
        {
            this.queue.Enqueue(message);
        }
    }
}
