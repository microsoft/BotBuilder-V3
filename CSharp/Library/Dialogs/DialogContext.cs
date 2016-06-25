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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class DialogContext : IDialogContext
    {
        private readonly IBotToUser botToUser;
        private readonly IBotData botData;
        private readonly IDialogStack stack;

        public DialogContext(IBotToUser botToUser, IBotData botData, IDialogStack stack)
        {
            SetField.NotNull(out this.botToUser, nameof(botToUser), botToUser);
            SetField.NotNull(out this.botData, nameof(botData), botData);
            SetField.NotNull(out this.stack, nameof(stack), stack);
        }

        IBotDataBag IBotData.ConversationData
        {
            get
            {
                return this.botData.ConversationData;
            }
        }

        IBotDataBag IBotData.PrivateConversationData
        {
            get
            {
                return this.botData.PrivateConversationData;
            }
        }

        IBotDataBag IBotData.UserData
        {
            get
            {
                return this.botData.UserData;
            }
        }

        async Task IBotToUser.PostAsync(IMessageActivity message, CancellationToken cancellationToken)
        {
            await this.botToUser.PostAsync(message, cancellationToken);
        }

        IMessageActivity IBotToUser.MakeMessage()
        {
            return this.botToUser.MakeMessage();
        }

        IReadOnlyList<Delegate> IDialogStack.Frames
        {
            get
            {
                return this.stack.Frames;
            }
        }

        void IDialogStack.Call<R>(IDialog<R> child, ResumeAfter<R> resume)
        {
            this.stack.Call<R>(child, resume);
        }

        async Task IDialogStack.Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token)
        {
            await this.stack.Forward<R,T>(child, resume, item, token);
        }

        void IDialogStack.Done<R>(R value)
        {
            this.stack.Done<R>(value);
        }

        void IDialogStack.Fail(Exception error)
        {
            this.stack.Fail(error);
        }

        void IDialogStack.Wait<R>(ResumeAfter<R> resume)
        {
            this.stack.Wait(resume);
        }

        async Task IDialogStack.PollAsync(CancellationToken token)
        {
            await this.stack.PollAsync(token);
        }

        void IDialogStack.Reset()
        {
            this.stack.Reset();
        }

        async Task IBotData.LoadAsync(CancellationToken cancellationToken)
        {
            await this.botData.LoadAsync(cancellationToken); 
        }

        async Task IBotData.FlushAsync(CancellationToken cancellationToken)
        {
            await this.botData.FlushAsync(cancellationToken); 
        }
    }
}
