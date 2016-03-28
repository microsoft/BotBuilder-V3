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
    /// <summary>   Simple way to call a sub-dialog with a resume handler for the result. </summary>
    /// <typeparam name="R">    Type of result expected from sub-dialog. </typeparam>
    [Serializable]
    public class CallDialog<R> : IDialog
    {
        /// <summary>   Resume handler when sub-dialog returns. </summary>
        /// <param name="dialog">   Parent dialog. </param>
        /// <param name="context">  Context. </param>
        /// <param name="result">   An empty task. </param>
        /// <returns>   A Task. </returns>
        public delegate Task Resume(CallDialog<R> dialog, IDialogContext context, IAwaitable<R> result);

        private readonly IDialog child;
        private readonly Resume resume;

        /// <summary>   Construct a CallDialog that calls child and passes the result to a resume handler.  </summary>
        /// <param name="child">    Child dialog. </param>
        /// <param name="resume">   Resume handler. </param>
        public CallDialog(IDialog child, Resume resume)
        {
            SetField.NotNull(out this.child, nameof(child), child);
            SetField.NotNull(out this.resume, nameof(resume), resume);
        }

        async Task IDialog.StartAsync(IDialogContext context)
        {
            await CallChild(context, ignored: null);
        }

        /// <summary>   Resume handler for when child sub-dialog is done. </summary>
        /// <param name="context">  Context. </param>
        /// <param name="result">   Result from child sub-dialog. </param>
        /// <returns>   A Task. </returns>
        public async Task ChildDone(IDialogContext context, IAwaitable<R> result)
        {
            await resume(this, context, result);
        }

        /// <summary>   Call child sub-dialog. </summary>
        /// <param name="context">  Context. </param>
        /// <param name="ignored">  Ignored. </param>
        /// <returns>   A Task. </returns>
        public async Task CallChild(IDialogContext context, IAwaitable<object> ignored)
        {
            context.Call<R>(this.child, ChildDone);
        }
    }
}
