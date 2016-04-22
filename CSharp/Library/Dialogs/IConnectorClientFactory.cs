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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// Factory for IConnectorClient.
    /// </summary>
    public interface IConnectorClientFactory
    {
        /// <summary>
        /// Make the IConnectorClient implementation.
        /// </summary>
        /// <returns>The IConnectorClient implementation.</returns>
        IConnectorClient Make();
    }

    /// <summary>
    /// Type of the connector deployment that the bot is talking to.
    /// </summary>
    public enum ConnectorType
    {
        Emulator, 
        Cloud
    }

    public sealed class DetectEmulatorFactory : IConnectorClientFactory
    {
        private readonly Uri emulator;
        private readonly bool? isEmulator; 
        public DetectEmulatorFactory(Message message, Uri emulator)
        {
            SetField.CheckNull(nameof(message), message);
            var channel = message.From;
            this.isEmulator = channel?.ChannelId?.Equals("emulator", StringComparison.OrdinalIgnoreCase);
            SetField.NotNull(out this.emulator, nameof(emulator), emulator);
        }

        public DetectEmulatorFactory(ConnectorType connectorType, Uri emulator)
        {
            this.isEmulator = connectorType == ConnectorType.Emulator; 
            SetField.NotNull(out this.emulator, nameof(emulator), emulator);
        }

        IConnectorClient IConnectorClientFactory.Make()
        {
            if (isEmulator ?? false)
            {
                return new ConnectorClient(this.emulator, new ConnectorClientCredentials());
            }
            else
            {
                return new ConnectorClient();
            }
        }
    }

    /// <summary>
    /// connector client extensions.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Loads the message data from connector. 
        /// </summary>
        /// <param name="client"> Instance of connector client.</param>
        /// <param name="botId"> Id of the bot.</param>
        /// <param name="userId"> Id of the user.</param>
        /// <param name="conversationId"> Id of the conversation.</param>
        /// <param name="token"> The cancelation token.</param>
        /// <returns> A message with appropriate data fields.</returns>
        public static async Task<Message> LoadMessageData(this IConnectorClient client, string botId, string userId, string conversationId, CancellationToken token = default(CancellationToken))
        {
            var continuationMessage = new Message
            {
                ConversationId = conversationId,
                To = new ChannelAccount
                {
                    Id = botId
                },
                From = new ChannelAccount
                {
                    Id = userId
                }
            };

            var dataRetrievalTasks = new List<Task<BotData>> {
                client.Bots.GetConversationDataAsync(botId, conversationId, token),
                client.Bots.GetUserDataAsync(botId, userId, token),
                client.Bots.GetPerUserConversationDataAsync(botId, conversationId, userId, token)
            } ;


            await Task.WhenAll(dataRetrievalTasks); 

            continuationMessage.BotConversationData = dataRetrievalTasks[0].Result?.Data;
            continuationMessage.BotUserData = dataRetrievalTasks[1].Result?.Data;
            continuationMessage.BotPerUserInConversationData = dataRetrievalTasks[2].Result?.Data;

            return continuationMessage;
        }
    }
}
