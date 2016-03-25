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
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder
{
    public static partial class CompositionRoot
    {
        private const string BlobKey = "DialogState";

        public static BinaryFormatter MakeBinaryFormatter(IServiceProvider provider)
        {
            var listener = new DefaultTraceListener();
            var reference = new Serialization.LogSurrogate(new Serialization.ReferenceSurrogate(), listener);
            //var reflection = new Serialization.LogSurrogate(new Serialization.ReflectionSurrogate(), listener);
            var selector = new Serialization.SurrogateSelector(reference, reflection: null);
            var context = new StreamingContext(StreamingContextStates.All, provider);
            var formatter = new BinaryFormatter(selector, context);
            return formatter;
        }

        public static async Task<Message> SendAsync<T>(Message toBot, Func<IDialog<T>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            var waits = new WaitFactory();
            var frames = new FrameFactory(waits);
            IBotData toBotData = new Internals.JObjectBotData(toBot);
            IConnectorClient client = new ConnectorClient();
            var provider = new Serialization.SimpleServiceLocator()
            {
                waits, frames, toBotData, client
            };
            var formatter = CompositionRoot.MakeBinaryFormatter(provider);

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
                var loop = Methods.Void(Methods.Loop(context.ToRest<T>(root.StartAsync), int.MaxValue));
                fiber.Call(loop, default(T));
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
