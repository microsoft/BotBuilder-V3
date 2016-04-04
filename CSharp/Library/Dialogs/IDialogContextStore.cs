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

using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// Methods for loading and saving the dialog context.
    /// </summary>
    public interface IDialogContextStore
    {
        /// <summary>
        /// Try to load the dialog context.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool TryLoad(out IDialogContextInternal context);

        /// <summary>
        /// Save the dialog context.
        /// </summary>
        /// <param name="context">The dialog context.</param>
        void Save(IDialogContextInternal context);
    }

    /// <summary>
    /// Helper methods for dialog contexts.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Load or create the dialog context from the store.
        /// </summary>
        /// <typeparam name="T">The type of the root dialog.</typeparam>
        /// <param name="store">The dialog context store.</param>
        /// <param name="MakeRoot">The factory method for the root dialog.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>A task representing the dialog context load operation.</returns>
        public static async Task<IDialogContextInternal> LoadAsync<T>(this IDialogContextStore store, Func<IDialog<T>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            IDialogContextInternal context;
            if (!store.TryLoad(out context))
            {
                var root = MakeRoot();
                var loop = root.Loop();
                context.Call(loop, null);
                await context.PollAsync();
            }

            return context;
        }

        /// <summary>
        /// Poll the dialog context for any work to be done.
        /// </summary>
        /// <typeparam name="T">The type of the root dialog.</typeparam>
        /// <param name="store">The dialog context store.</param>
        /// <param name="MakeRoot">The factory method for the root dialog.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>A task representing the poll operation.</returns>
        public static async Task PollAsync<T>(this IDialogContextStore store, Func<IDialog<T>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            var context = await LoadAsync(store, MakeRoot);
            store.Save(context);
        }

        /// <summary>
        /// Post a message to the dialog context and poll the dialog context for any work to be done.
        /// </summary>
        /// <typeparam name="T">The type of the root dialog.</typeparam>
        /// <param name="store">The dialog context store.</param>
        /// <param name="toBot">The message sent to the bot.</param>
        /// <param name="MakeRoot">The factory method for the root dialog.</param>
        /// <param name="token">An optional cancellation token.</param>
        /// <returns>A task representing the post operation.</returns>
        public static async Task PostAsync<T>(this IDialogContextStore store, Message toBot, Func<IDialog<T>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            var context = await LoadAsync(store, MakeRoot);
            IUserToBot userToBot = context;
            await userToBot.PostAsync(toBot, token);
            store.Save(context);
        }
    }
}
