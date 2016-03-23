using Microsoft.Bot.Builder.Fibers;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;

namespace Microsoft.Bot.Builder
{
#pragma warning disable CS1998

    [Serializable]
    public sealed class DialogContext : IDialogContext, IUserToBot, ISerializable
    {
        private readonly IBotData data;
        private readonly IFiberLoop fiber;

        public DialogContext(IBotData data, IFiberLoop fiber)
        {
            Field.SetNotNull(out this.data, nameof(data), data);
            Field.SetNotNull(out this.fiber, nameof(fiber), fiber);
        }

        public DialogContext(SerializationInfo info, StreamingContext context)
        {
            Field.SetNotNullFrom(out this.data, nameof(data), info);
            Field.SetNotNullFrom(out this.fiber, nameof(fiber), info);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.data), this.data);
            info.AddValue(nameof(this.fiber), this.fiber);
        }

        IBotDataBag IBotData.ConversationData
        {
            get
            {
                return this.data.ConversationData;
            }
        }

        IBotDataBag IBotData.PerUserInConversationData
        {
            get
            {
                return this.data.PerUserInConversationData;
            }
        }

        IBotDataBag IBotData.UserData
        {
            get
            {
                return this.data.UserData;
            }
        }

        private IWait wait;

        [Serializable]
        private sealed class Thunk<T>
        {
            private DialogContext context;
            private ResumeAfter<T> resume;

            public Thunk(DialogContext context, ResumeAfter<T> resume)
            {
                Field.SetNotNull(out this.context, nameof(context), context);
                Field.SetNotNull(out this.resume, nameof(resume), resume);
            }

            public async Task<IWait> Rest(IFiber fiber, IItem<T> item)
            {
                await this.resume(this.context, item);
                return this.context.wait;
            }
        }

        public Rest<T> ToRest<T>(ResumeAfter<T> resume)
        {
            var thunk = new Thunk<T>(this, resume);
            return thunk.Rest;
        }

        void IDialogStackNew.Call<T, R>(T child, ResumeAfter<R> resume)
        {
            this.wait = this.fiber.Call<T, R>(ToRest<object>(child.StartAsync), null, ToRest(resume));
        }

        void IDialogStackNew.Done<R>(R value)
        {
            this.wait = this.fiber.Done(value);
        }

        void IDialogStackNew.Wait(ResumeAfter<Message> resume)
        {
            this.wait = this.fiber.Wait<Message>(ToRest(resume));
        }

        private Message toUser;

        async Task IBotToUser.PostAsync(Message message, CancellationToken cancellationToken)
        {
            Field.SetNotNull(out this.toUser, nameof(message), message);
        }

        private Message toBot;

        async Task<Message> IUserToBot.PostAsync(Message message, CancellationToken cancellationToken)
        {
            this.toBot = message;
            this.fiber.Post(message);
            await this.fiber.PollAsync();
            return toUser;
        }

        public static Message ToUser(Message toBot, string toUserText)
        {
            if (toBot != null)
            {
                var toUser = toBot.CreateReplyMessage(toUserText);
                toUser.BotUserData = toBot.BotUserData;
                toUser.BotConversationData = toBot.BotConversationData;
                toUser.BotPerUserInConversationData = toBot.BotPerUserInConversationData;

                return toUser;
            }
            else
            {
                return new Message(text: toUserText);
            }
        }

        async Task IDialogContext.PostAsync(string text, CancellationToken cancellationToken)
        {
            var toUser = DialogContext.ToUser(this.toBot, text);
            IBotToUser botToUser = this;
            await botToUser.PostAsync(toUser, cancellationToken);
        }
    }

    public static partial class CompositionRoot
    {
        private const string BlobKey = "DialogState";

        public static async Task<HttpResponseMessage> PostAsync(HttpRequestMessage request, Message toBot, Func<IDialogNew> MakeRoot, params Serialization.ISerializeAsReference[] singletons)
        {
            try
            {
                var toUser = await PostAsync(toBot, MakeRoot, singletons);

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
            var reflection = new Serialization.LogSurrogate(new Serialization.ReflectionSurrogate(), listener);
            var selector = new Serialization.SurrogateSelector(reference, reflection);
            var context = new StreamingContext(StreamingContextStates.All, provider);
            var formatter = new BinaryFormatter(selector, context);
            return formatter;
        }

        public sealed class SimpleServiceLocator : IServiceProvider, IEnumerable<object>
        {
            private readonly Dictionary<Type, object> instanceByType;

            public SimpleServiceLocator(IEnumerable<object> instances = null)
            {
                instances = instances ?? Enumerable.Empty<object>();
                this.instanceByType = instances.ToDictionary(o => o.GetType(), o => o);
            }

            public void Add(object instance)
            {
                var type = instance.GetType();
                this.instanceByType.Add(type, instance);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.instanceByType.Values.GetEnumerator();
            }

            IEnumerator<object> IEnumerable<object>.GetEnumerator()
            {
                return this.instanceByType.Values.GetEnumerator();
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                return this.instanceByType[serviceType];
            }
        }

        public static async Task<Message> PostAsync(Message toBot, Func<IDialogNew> MakeRoot, params Serialization.ISerializeAsReference [] singletons)
        {
            var waits = new WaitFactory();
            var frames = new FrameFactory(waits);
            IBotData toBotData = new JObjectBotData(toBot);
            var provider = new SimpleServiceLocator(singletons)
            {
                waits, frames, toBotData
            };
            var formatter = MakeBinaryFormatter(provider);

            DialogContext context;

            byte[] blobOld;
            bool found = toBotData.PerUserInConversationData.TryGetValue(BlobKey, out blobOld);
            if (found)
            {
                using (var streamOld = new MemoryStream(blobOld))
                {
                    context = (DialogContext)formatter.Deserialize(streamOld);
                }
            }
            else
            {
                IFiberLoop fiber = new Fiber(frames);
                context = new DialogContext(toBotData, fiber);
                var root = MakeRoot();
                var loop = Methods.Void(Methods.Loop(context.ToRest<object>(root.StartAsync), int.MaxValue));
                fiber.Call(loop, null);
                await fiber.PollAsync();
            }

            IUserToBot userToBot = context;
            var toUser = await userToBot.PostAsync(toBot, CancellationToken.None);

            // even with no bot response, try to save state
            if (toUser == null)
            {
                toUser = DialogContext.ToUser(toBot, toUserText: null);
            }

            using (var streamNew = new MemoryStream())
            {
                formatter.Serialize(streamNew, context);
                var blobNew = streamNew.ToArray();
                IBotData toUserData = new JObjectBotData(toUser);

                toUserData.PerUserInConversationData.SetValue(BlobKey, blobNew);
            }

            return toUser;
        }
    }

    public static partial class Extensions
    {
        public static void Call<T, R>(this IDialogContext context, ResumeAfter<R> resume) where T : class, IDialogNew, new()
        {
            context.Call<T, R>(new T(), resume);
        }
    }
}
