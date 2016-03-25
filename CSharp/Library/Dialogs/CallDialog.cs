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
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{

    [Serializable]
    public class CallDialog<T, R> : IDialog<T>
    {
        public delegate Task Resume(CallDialog<T, R> dialog, IDialogContext context, IAwaitable<R> result);

        private readonly IDialog<T> child;
        private readonly Resume resume;
        private T argument;

        public CallDialog(IDialog<T> child, Resume resume)
        {
            Field.SetNotNull(out this.child, nameof(child), child);
            Field.SetNotNull(out this.resume, nameof(resume), resume);
        }

        async Task IDialog<T>.StartAsync(IDialogContext context, IAwaitable<T> argument)
        {
            this.argument = await argument;
            await CallChild(context, null);
        }

        private async Task ChildDone(IDialogContext context, IAwaitable<R> result)
        {
            await resume(this, context, result);
        }

        public async Task CallChild(IDialogContext context, IAwaitable<object> ignored)
        {
            context.Call<IDialog<T>, T, R>(this.child, this.argument, ChildDone);
        }
    }
}
