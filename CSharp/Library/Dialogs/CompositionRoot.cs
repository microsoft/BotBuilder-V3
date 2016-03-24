using Microsoft.Bot.Builder.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public static partial class CompositionRoot
    {
        private const string BlobKey = "DialogState";

        public static async Task<HttpResponseMessage> PostAsync<T>(HttpRequestMessage request, Message toBot, Func<IDialog<T>> MakeRoot)
        {
            try
            {
                var toUser = await PostAsync(toBot, MakeRoot);

                return request.CreateResponse(toUser);
            }
            catch (Exception error)
            {
                return request.CreateResponse(error);
            }
        }

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

        public static async Task<Message> PostAsync<T>(Message toBot, Func<IDialog<T>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            var waits = new WaitFactory();
            var frames = new FrameFactory(waits);
            IBotData toBotData = new Internals.JObjectBotData(toBot);
            var provider = new Serialization.SimpleServiceLocator()
            {
                waits, frames, toBotData
            };
            var formatter = CompositionRoot.MakeBinaryFormatter(provider);

            DialogContext context;

            byte[] blobOld;
            bool found = toBotData.PerUserInConversationData.TryGetValue(BlobKey, out blobOld);
            if (found)
            {
                using (var streamOld = new MemoryStream(blobOld))
                using (var gzipOld = new GZipStream(streamOld, CompressionMode.Decompress))
                {
                    context = (DialogContext)formatter.Deserialize(gzipOld);
                }
            }
            else
            {
                IFiberLoop fiber = new Fiber(frames);
                context = new DialogContext(toBotData, fiber);
                var root = MakeRoot();
                var loop = Methods.Void(Methods.Loop(context.ToRest<T>(root.StartAsync), int.MaxValue));
                fiber.Call(loop, default(T));
                await fiber.PollAsync();
            }

            IUserToBot userToBot = context;
            var toUser = await userToBot.PostAsync(toBot, token);

            // even with no bot response, try to save state
            if (toUser == null)
            {
                toUser = DialogContext.ToUser(toBot, toUserText: null);
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
