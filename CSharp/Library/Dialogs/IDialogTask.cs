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

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// The stack of dialogs in the conversational process.
    /// </summary>
    public interface IDialogStack
    {
        /// <summary>
        /// The dialog frames active on the stack.
        /// </summary>
        IReadOnlyList<Delegate> Frames { get; }

        /// <summary>
        /// Suspend the current dialog until an external event has been sent to the bot.
        /// </summary>
        /// <param name="resume">The method to resume when the event has been received.</param>
        void Wait<R>(ResumeAfter<R> resume);

        /// <summary>
        /// Call a child dialog and add it to the top of the stack.
        /// </summary>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <param name="child">The child dialog.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        void Call<R>(IDialog<R> child, ResumeAfter<R> resume);

        /// <summary>
        /// Call a child dialog, add it to the top of the stack and post the item to the child dialog.
        /// </summary>
        /// <typeparam name="R">The type of result expected from the child dialog.</typeparam>
        /// <typeparam name="T">The type of the item posted to child dialog.</typeparam>
        /// <param name="child">The child dialog.</param>
        /// <param name="resume">The method to resume when the child dialog has completed.</param>
        /// <param name="item">The item that will be posted to child dialog.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the Forward operation.</returns>
        Task Forward<R, T>(IDialog<R> child, ResumeAfter<R> resume, T item, CancellationToken token);

        /// <summary>
        /// Complete the current dialog and return a result to the parent dialog.
        /// </summary>
        /// <typeparam name="R">The type of the result dialog.</typeparam>
        /// <param name="value">The value of the result.</param>
        void Done<R>(R value);

        /// <summary>
        /// Fail the current dialog and return an exception to the parent dialog.
        /// </summary>
        /// <param name="error">The error.</param>
        void Fail(Exception error);

        /// <summary>
        /// Poll the dialog task for any work to be done.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the poll operation.</returns>
        Task PollAsync(CancellationToken token);

        /// <summary>
        /// Resets the stack.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// Methods to send a message from the user to the bot.
    /// </summary>
    public interface IPostToBot
    {
        /// <summary>
        /// Post an item (e.g. message or other external event) to the bot.
        /// </summary>
        /// <param name="item">The item for the bot.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        Task PostAsync<T>(T item, CancellationToken token);
    }
}