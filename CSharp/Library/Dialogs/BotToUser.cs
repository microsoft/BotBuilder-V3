// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using System.IO;
using System.Linq; 
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class SendLastInline_BotToUser : IBotToUser
    {
        private readonly Message toBot;
        private readonly IConnectorClient client;
        public SendLastInline_BotToUser(Message toBot, IConnectorClient client)
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

    public sealed class AlwaysSendDirect_BotToUser : IBotToUser
    {
        private readonly Message toBot;
        private readonly IConnectorClient client;
        public AlwaysSendDirect_BotToUser(Message toBot, IConnectorClient client)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
            SetField.NotNull(out this.client, nameof(client), client);
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
            await this.client.Messages.SendMessageAsync(message, cancellationToken);
        }
    }

    public sealed class BotToUserQueue : IBotToUser
    {
        private readonly Message toBot;
        private readonly Queue<Message> queue;
        public BotToUserQueue(Message toBot, Queue<Message> queue)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
            SetField.NotNull(out this.queue, nameof(queue), queue);
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

    public sealed class BotToUserTextWriter : IBotToUser
    {
        private readonly IBotToUser inner;
        private readonly TextWriter writer;
        public BotToUserTextWriter(IBotToUser inner, TextWriter writer)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.writer, nameof(writer), writer);
        }

        Message IBotToUser.MakeMessage()
        {
            return this.inner.MakeMessage();
        }

        async Task IBotToUser.PostAsync(Message message, CancellationToken cancellationToken)
        {
            await this.inner.PostAsync(message, cancellationToken);
            await this.writer.WriteLineAsync($"{message.Text}{ActionsToText(message.Attachments)}");
        }

        private static string ActionsToText(IList<Attachment> attachments)
        {
            var buttonAttachments = attachments?.Where(attachment => attachment.Actions?.Count > 0);
            string text = string.Empty;
            if (buttonAttachments != null)
            {
                foreach (var attachment in buttonAttachments)
                {
                    if (attachment != null && attachment.Actions != null && attachment.Actions.Count() > 0)
                    {
                        foreach (var action in attachment.Actions)
                        {
                            if (!string.IsNullOrEmpty(action.Message))
                            {
                                text += $"\n{action.Message}. {action.Title}";
                            }
                            else
                            { 
                                text += $"\n* {action.Title}";
                            }
                        }
                    }
                }
            }
            return text;
        }
    }
}
