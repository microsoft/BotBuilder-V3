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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The top level composition root for the SDK.
    /// </summary>
    public static partial class Conversation
    {
        public static readonly IContainer Container;

        static Conversation()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new Internals.DialogModule());
            if (Debugger.IsAttached)
            {
                builder
                    .Register(c => new ConnectorClient(new Uri("http://localhost:9000"), new ConnectorClientCredentials()))
                    .As<IConnectorClient>()
                    .SingleInstance();
            }

            Container = builder.Build();
        }

        /// <summary>
        /// Process an incoming message within the conversation.
        /// </summary>
        /// <remarks>
        /// This method:
        /// 1. instantiates and composes the required components
        /// 2. deserializes the dialog state (the dialog stack and each dialog's state) from the <see cref="toBot"/> <see cref="Message"/>
        /// 3. resumes the conversation processes where the dialog suspended to wait for a <see cref="Message"/>
        /// 4. queues <see cref="Message"/>s to be sent to the user
        /// 5. serializes the updated dialog state in the messages to be sent to the user.
        /// 
        /// The <see cref="MakeRoot"/> factory method is invoked for new conversations only,
        /// because existing conversations have the dialog stack and state serialized in the <see cref="Message"/> data.
        /// </remarks>
        /// <param name="toBot">The message sent to the bot.</param>
        /// <param name="MakeRoot">The factory method to make the root dialog.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the message to send inline back to the user.</returns>
        public static async Task<Message> SendAsync<T>(Message toBot, Func<IDialog<T>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));
                await store.PostAsync<T>(toBot, MakeRoot, token);

                var botToUser = scope.Resolve<SendLastInline_BotToUser>();
                return botToUser.ToUser;
            }
        }
    }
}
