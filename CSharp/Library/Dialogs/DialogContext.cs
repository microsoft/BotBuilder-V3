using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Internals
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

        void IDialogStack.Call<C, T, R>(C child, T arguments, ResumeAfter<R> resume)
        {
            var callRest = ToRest<T>(child.StartAsync);
            var doneRest = ToRest(resume);
            this.wait = this.fiber.Call<T, R>(callRest, arguments, doneRest);
        }

        void IDialogStack.Done<R>(R value)
        {
            this.wait = this.fiber.Done(value);
        }

        void IDialogStack.Wait(ResumeAfter<Message> resume)
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
}
