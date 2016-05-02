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

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Internals.Fibers;

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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        Task PostAsync<T>(T item, CancellationToken cancellationToken = default(CancellationToken));
    }

    /// <summary>
    /// A dialog stack and a method to communicate from the outside world to the dialogs running on the stack.
    /// </summary>
    public interface IDialogTask : IDialogStack, IPostToBot
    {
        /// <summary>
        /// Poll the dialog stack for any work to be done.
        /// </summary>
        /// <returns>A task that represents the poll operation.</returns>
        Task PollAsync();

        /// <summary>
        /// Reset the backing store.
        /// </summary>
        void Reset();

        /// <summary>
        /// Save the dialog task to its backing store.
        /// </summary>
        void Save();
    }

    public abstract class DelegatingDialogTask : IDialogTask
    {
        private readonly IDialogTask inner;
        protected DelegatingDialogTask(IDialogTask inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        public virtual IReadOnlyList<Delegate> Frames
        {
            get
            {
                return this.inner.Frames;
            }
        }

        public virtual void Call<R>(IDialog<R> child, ResumeAfter<R> resume)
        {
            this.inner.Call<R>(child, resume);
        }

        public virtual void Done<R>(R value)
        {
            this.inner.Done<R>(value);
        }

        public virtual void Fail(Exception error)
        {
            this.inner.Fail(error);
        }

        public virtual async Task PollAsync()
        {
            await this.inner.PollAsync();
        }

        public virtual async Task PostAsync<T>(T item, CancellationToken cancellationToken = default(CancellationToken))
        {
            await this.inner.PostAsync<T>(item, cancellationToken);
        }

        public virtual void Reset()
        {
            this.inner.Reset();
        }

        public virtual void Save()
        {
            this.inner.Save();
        }

        public virtual void Wait<R>(ResumeAfter<R> resume)
        {
            this.inner.Wait(resume);
        }
    }

    /// <summary>
    /// Helper methods for dialog contexts.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Load or create the dialog task from the store.
        /// </summary>
        /// <typeparam name="R">The type of the root dialog.</typeparam>
        /// <param name="task">The dialog task.</param>
        /// <param name="MakeRoot">The factory method for the root dialog.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>A task representing the dialog task load operation.</returns>
        public static async Task LoadAsync<R>(this IDialogTask task, Func<IDialog<R>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            if (task.Frames.Count == 0)
            {
                var root = MakeRoot();
                var loop = root.Loop();
                task.Call(loop, null);
                await task.PollAsync();
            }
        }

        /// <summary>
        /// Poll the dialog task for any work to be done.
        /// </summary>
        /// <typeparam name="R">The type of the root dialog.</typeparam>
        /// <param name="task">The dialog task.</param>
        /// <param name="MakeRoot">The factory method for the root dialog.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>A task representing the poll operation.</returns>
        public static async Task PollAsync<R>(this IDialogTask task, Func<IDialog<R>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            await LoadAsync(task, MakeRoot);
            task.Save();
        }

        /// <summary>
        /// Post an item to the dialog task and poll the dialog task for any work to be done.
        /// </summary>
        /// <typeparam name="R">The type of the root dialog.</typeparam>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="task">The dialog task.</param>
        /// <param name="toBot">The item to be sent to the bot.</param>
        /// <param name="MakeRoot">The factory method for the root dialog.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>A task representing the post operation.</returns>
        public static async Task PostAsync<T, R>(this IDialogTask task, T toBot, Func<IDialog<R>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            await LoadAsync(task, MakeRoot, token);

            IPostToBot postToBot = task;
            try
            {
                await postToBot.PostAsync(toBot, token);
            }
            catch
            {
                task.Reset();
                throw;
            }

            task.Save();
        }

        /// <summary>
        /// <see cref="PostAsync{T, R}(IDialogTask, T, Func{IDialog{R}}, CancellationToken)"/>
        /// </summary>
        /// <remarks> 
        /// This function trys to resume the conversation based on the call stack. 
        /// It throws <see cref="InvalidOperationException"/> if loading the call stack from context store fails.
        /// </remarks>
        /// <typeparam name="T"> The type of the item.</typeparam>
        /// <param name="task"> The dialog task.</param>
        /// <param name="toBot"> The item to be sent to the bot.</param>
        /// <param name="token"> An optional cancellation token.</param>
        /// <returns> A task representing the post operation.</returns>
        public static async Task PostAsync<T>(this IDialogTask task, T toBot, CancellationToken token = default(CancellationToken))
        {
            await PostAsync<T, object>(task, toBot, null, token);
        }
    }
}