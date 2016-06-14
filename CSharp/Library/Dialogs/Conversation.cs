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

using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

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
            builder.RegisterModule(new DialogModule_MakeRoot());
            Container = builder.Build();
        }

        /// <summary>
        /// Process an incoming message within the conversation.
        /// </summary>
        /// <remarks>
        /// This method:
        /// 1. Instantiates and composes the required components.
        /// 2. Deserializes the dialog state (the dialog stack and each dialog's state) from the <paramref name="toBot"/> <see cref="IMessageActivity"/>.
        /// 3. Resumes the conversation processes where the dialog suspended to wait for a <see cref="IMessageActivity"/>.
        /// 4. Queues <see cref="IMessageActivity"/>s to be sent to the user.
        /// 5. Serializes the updated dialog state in the messages to be sent to the user.
        /// 
        /// The <paramref name="MakeRoot"/> factory method is invoked for new conversations only,
        /// because existing conversations have the dialog stack and state serialized in the <see cref="IMessageActivity"/> data.
        /// </remarks>
        /// <param name="toBot">The message sent to the bot.</param>
        /// <param name="MakeRoot">The factory method to make the root dialog.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the message to send inline back to the user.</returns>
        public static async Task SendAsync(IMessageActivity toBot, Func<IDialog<object>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            using (var scope = DialogModule.BeginLifetimeScope(Container, toBot))
            {
                DialogModule_MakeRoot.Register(scope, MakeRoot);
                await SendAsync(scope, toBot, token);
            }
        }

        /// <summary>
        /// Resume a conversation and post the data to the dialog waiting.
        /// </summary>
        /// <typeparam name="T"> Type of the data. </typeparam>
        /// <param name="resumptionCookie"> The id of the bot.</param>
        /// <param name="toBot"> The data sent to bot.</param>
        /// <param name="token"> The cancellation token.</param>
        /// <returns> A task that represent the message to send back to the user after resumption of the conversation.</returns>
        public static async Task ResumeAsync<T>(ResumptionCookie resumptionCookie, T toBot, CancellationToken token = default(CancellationToken))
        {
            var continuationMessage = resumptionCookie.GetMessage(); 
            using (var scope = DialogModule.BeginLifetimeScope(Container, continuationMessage))
            {
                Func<IDialog<object>> MakeRoot = () => { throw new InvalidOperationException(); };
                DialogModule_MakeRoot.Register(scope, MakeRoot);

                await ResumeAsync(scope, continuationMessage, toBot, token);
            }
        }

        internal static async Task SendAsync(ILifetimeScope scope, IMessageActivity toBot, CancellationToken token = default(CancellationToken))
        {
            using (new LocalizedScope(toBot.Locale))
            {
                var task = scope.Resolve<IPostToBot>();
                await task.PostAsync(toBot, token);
            }
        }

        internal static async Task ResumeAsync<T>(ILifetimeScope scope, IMessageActivity continuationMessage, T toBot, CancellationToken token = default(CancellationToken))
        {
            var client = scope.Resolve<IConnectorClient>();
            var botData = scope.Resolve<IBotData>();
            await botData.LoadAsync(token); 

            using (new LocalizedScope(continuationMessage.Locale))
            {
                var task = scope.Resolve<IPostToBot>();
                await task.PostAsync(toBot, token);
            }
        }
    }
}