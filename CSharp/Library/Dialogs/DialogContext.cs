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
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    [Serializable]
    public sealed class DialogContext : IDialogContextInternal, ISerializable
    {
        private readonly IBotToUser botToUser;
        private readonly IBotData data;
        private readonly IFiberLoop fiber;

        public DialogContext(IBotToUser botToUser, IBotData data, IFiberLoop fiber)
        {
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.data, nameof(data), data);
            SetField.NotNull(out this.fiber, nameof(fiber), fiber);
        }

        public DialogContext(SerializationInfo info, StreamingContext context)
        {
            SetField.NotNullFrom(out this.botToUser, nameof(botToUser), info);
            SetField.NotNullFrom(out this.data, nameof(data), info);
            SetField.NotNullFrom(out this.fiber, nameof(fiber), info);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.botToUser), this.botToUser);
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
        private sealed class ThunkStart
        {
            private DialogContext context;
            private StartAsync start;

            public ThunkStart(DialogContext context, StartAsync start)
            {
                SetField.NotNull(out this.context, nameof(context), context);
                SetField.NotNull(out this.start, nameof(start), start);
            }

            public async Task<IWait> Rest(IFiber fiber, IItem<object> item)
            {
                var result = await item;
                if (result != null)
                {
                    throw new ArgumentException(nameof(item));
                }

                await this.start(this.context);
                return this.context.wait;
            }
        }

        [Serializable]
        private sealed class ThunkResume<T>
        {
            private DialogContext context;
            private ResumeAfter<T> resume;

            public ThunkResume(DialogContext context, ResumeAfter<T> resume)
            {
                SetField.NotNull(out this.context, nameof(context), context);
                SetField.NotNull(out this.resume, nameof(resume), resume);
            }

            public async Task<IWait> Rest(IFiber fiber, IItem<T> item)
            {
                await this.resume(this.context, item);
                return this.context.wait;
            }
        }

        internal Rest<object> ToRest(StartAsync start)
        {
            var thunk = new ThunkStart(this, start);
            return thunk.Rest;
        }

        internal Rest<T> ToRest<T>(ResumeAfter<T> resume)
        {
            var thunk = new ThunkResume<T>(this, resume);
            return thunk.Rest;
        }

        void IDialogStack.Call<R>(IDialog<R> child, ResumeAfter<R> resume)
        {
            var callRest = ToRest(child.StartAsync);
            if (resume != null)
            {
                var doneRest = ToRest(resume);
                this.wait = this.fiber.Call<object, R>(callRest, null, doneRest);
            }
            else
            {
                this.wait = this.fiber.Call<object>(callRest, null);
            }
        }

        void IDialogStack.Done<R>(R value)
        {
            this.wait = this.fiber.Done(value);
        }

        void IDialogStack.Wait(ResumeAfter<Message> resume)
        {
            this.wait = this.fiber.Wait<Message>(ToRest(resume));
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken cancellationToken)
        {
            this.fiber.Post(item);
            await this.fiber.PollAsync();
        }

        async Task IBotToUser.PostAsync(Message message, CancellationToken cancellationToken)
        {
            await this.botToUser.PostAsync(message, cancellationToken);
        }

        Message IBotToUser.MakeMessage()
        {
            return this.botToUser.MakeMessage();
        }

        async Task IDialogContextInternal.PollAsync()
        {
            await this.fiber.PollAsync();
        }
    }
}
