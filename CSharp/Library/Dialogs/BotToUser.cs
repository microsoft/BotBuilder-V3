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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class AlwaysSendDirect_BotToUser : IBotToUser
    {
        private readonly IMessageActivity toBot;
        private readonly IConnectorClient client;
        public AlwaysSendDirect_BotToUser(IMessageActivity toBot, IConnectorClient client)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
            SetField.NotNull(out this.client, nameof(client), client);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            var toBotActivity = (Activity)this.toBot;
            return toBotActivity.CreateReply();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.client.Conversations.ReplyToActivityAsync((Activity)message, cancellationToken);
        }
    }

    public sealed class BotToUserQueue : IBotToUser
    {
        private readonly IMessageActivity toBot;
        private readonly Queue<IMessageActivity> queue;
        public BotToUserQueue(IMessageActivity toBot, Queue<IMessageActivity> queue)
        {
            SetField.NotNull(out this.toBot, nameof(toBot), toBot);
            SetField.NotNull(out this.queue, nameof(queue), queue);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            var toBotActivity = (Activity)this.toBot;
            return toBotActivity.CreateReply();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
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

        IMessageActivity IBotToUser.MakeMessage()
        {
            return this.inner.MakeMessage();
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.inner.PostAsync(message, cancellationToken);
            await this.writer.WriteLineAsync($"{message.Text}{ButtonsToText(message.Attachments)}");
        }

        private static string ButtonsToText(IList<Attachment> attachments)
        {
            var cardAttachments = attachments?.Where(attachment => attachment.ContentType.StartsWith("application/vnd.microsoft.card"));
            var builder = new StringBuilder();
            if (cardAttachments != null && cardAttachments.Any())
            {
                builder.AppendLine(); 
                foreach (var attachment in cardAttachments)
                {
                    string type = attachment.ContentType.Split('.').Last();
                    if (type == "hero"  || type == "thumbnail")
                    {
                        var card = (HeroCard)attachment.Content;
                        if (!string.IsNullOrEmpty(card.Text))
                        {
                            builder.AppendLine(card.Text);
                        }
                        foreach(var button in card.Buttons)
                        {
                            if (!string.IsNullOrEmpty(button.Value))
                            {
                                builder.AppendLine($"{ button.Value}. { button.Title}");
                            }
                            else
                            {
                                builder.AppendLine($"* {button.Title}");
                            }
                        }
                    }
                }
            }
            return builder.ToString();
        }
    }
}
