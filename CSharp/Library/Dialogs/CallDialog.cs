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

using Microsoft.Bot.Builder.Fibers;
using System;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{

    [Serializable]
    public class CallDialog<R> : IDialog
    {
        public delegate Task Resume(CallDialog<R> dialog, IDialogContext context, IAwaitable<R> result);

        private readonly IDialog child;
        private readonly Resume resume;

        public CallDialog(IDialog child, Resume resume)
        {
            SetField.SetNotNull(out this.child, nameof(child), child);
            SetField.SetNotNull(out this.resume, nameof(resume), resume);
        }

        async Task IDialog.StartAsync(IDialogContext context)
        {
            await CallChild(context, ignored: null);
        }

        public async Task ChildDone(IDialogContext context, IAwaitable<R> result)
        {
            await resume(this, context, result);
        }

        public async Task CallChild(IDialogContext context, IAwaitable<object> ignored)
        {
            context.Call<R>(this.child, ChildDone);
        }
    }
}
