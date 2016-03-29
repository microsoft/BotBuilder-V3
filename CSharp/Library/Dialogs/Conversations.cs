﻿// 
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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// The top level composition root for the SDK.
    /// </summary>
    public static partial class Conversation
    {
        private const string BlobKey = "DialogState";

        /// <summary>Compose a BinaryFormatter within a singleton service provider.</summary>
        /// <param name="provider">The singleton service provider.</param>
        /// <returns>The BinaryFormatter.</returns>
        internal static BinaryFormatter MakeBinaryFormatter(IServiceProvider provider)
        {
            var listener = new DefaultTraceListener();
            var reference = new Serialization.LogSurrogate(new Serialization.ReferenceSurrogate(), listener);
            //var reflection = new Serialization.LogSurrogate(new Serialization.ReflectionSurrogate(), listener);
            var selector = new Serialization.SurrogateSelector(reference, reflection: null);
            var context = new StreamingContext(StreamingContextStates.All, provider);
            var formatter = new BinaryFormatter(selector, context);
            return formatter;
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
        /// <param name="singletons">An optional list of object instances that should not be serialized.</param>
        /// <returns></returns>
        public static async Task<Message> SendAsync(Message toBot, Func<IDialog> MakeRoot, CancellationToken token = default(CancellationToken), params object[] singletons)
        {
            IWaitFactory waits = new WaitFactory();
            IFrameFactory frames = new FrameFactory(waits);
            IBotData toBotData = new JObjectBotData(toBot);
            IConnectorClient client = new ConnectorClient();
            var provider = new Serialization.SimpleServiceLocator(singletons)
            {
                waits, frames, toBotData, client
            };
            var formatter = Conversation.MakeBinaryFormatter(provider);

            Internals.DialogContext context;

            byte[] blobOld;
            bool found = toBotData.PerUserInConversationData.TryGetValue(BlobKey, out blobOld);
            if (found)
            {
                using (var streamOld = new MemoryStream(blobOld))
                using (var gzipOld = new GZipStream(streamOld, CompressionMode.Decompress))
                {
                    context = (Internals.DialogContext)formatter.Deserialize(gzipOld);
                }
            }
            else
            {
                IFiberLoop fiber = new Fiber(frames);
                context = new Internals.DialogContext(client, toBotData, fiber);
                var root = MakeRoot();
                var loop = Methods.Void(Methods.Loop(context.ToRest(root.StartAsync), int.MaxValue));
                fiber.Call(loop, null);
                await fiber.PollAsync();
            }

            IUserToBot userToBot = context;
            var toUser = await userToBot.SendAsync(toBot, token);

            // even with no bot response, try to save state
            if (toUser == null)
            {
                toUser = Internals.DialogContext.ToUser(toBot, toUserText: null);
            }

            byte[] blobNew;
            using (var streamNew = new MemoryStream())
            using (var gzipNew = new GZipStream(streamNew, CompressionMode.Compress))
            {
                formatter.Serialize(gzipNew, context);
                gzipNew.Close();
                blobNew = streamNew.ToArray();
            }

            IBotData toUserData = new Internals.JObjectBotData(toUser);

            toUserData.PerUserInConversationData.SetValue(BlobKey, blobNew);

            return toUser;
        }
    }
}
