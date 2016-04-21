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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// Encapsulates a method that represents the code to execute after a result is available.
    /// </summary>
    /// <remarks>
    /// The result is often a message from the user.
    /// </remarks>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="context">The dialog context.</param>
    /// <param name="result">The result.</param>
    /// <returns>A task that represents the code that will resume after the result is available.</returns>
    public delegate Task ResumeAfter<in T>(IDialogContext context, IAwaitable<T> result);

    /// <summary>
    /// Encapsulate a method that represents the code to start a dialog.
    /// </summary>
    /// <param name="context">The dialog context.</param>
    /// <returns>A task that represents the start code for a dialog.</returns>
    public delegate Task StartAsync(IDialogContext context);

    /// <summary>
    /// The context for the bot.
    /// </summary>
    public interface IBotContext : IBotData, IBotToUser
    {
    }

    /// <summary>
    /// The context for the execution of a dialog's conversational process.
    /// </summary>
    public interface IDialogContext : IDialogStack, IBotContext
    {
    }

    /// <summary>
    /// Helper methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Post a message to be sent to the bot, using previous messages to establish a conversation context.
        /// </summary>
        /// <remarks>
        /// If the language parameter is not set, language of the incoming message will be used for reply.
        /// </remarks>
        /// <param name="botToUser">Communication channel to use.</param>
        /// <param name="text">The message text.</param>
        /// <param name="language">The language of the text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        public static async Task PostAsync(this IBotToUser botToUser, string text, string language = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = botToUser.MakeMessage();
            message.Text = text;
            
            if(!string.IsNullOrEmpty(language))
            {
                message.Language = language;
            }
            
            await botToUser.PostAsync(message, cancellationToken);
        }
    }
}

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// The stack of dialogs in the conversational process.
    /// </summary>
    public interface IDialogStack
    {
        /// <summary>
        /// Suspend the current dialog until the user has sent a message to the bot.
        /// </summary>
        /// <param name="resume">The method to resume when the message has been received.</param>
        void Wait(ResumeAfter<Message> resume);

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
    }

    /// <summary>
    /// Methods to send a message from the bot to the user. 
    /// </summary>
    public interface IBotToUser
    {
        /// <summary>
        /// Post a message to be sent to the user.
        /// </summary>
        /// <param name="message">The message for the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the post operation.</returns>
        Task PostAsync(Message message, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Make a message.
        /// </summary>
        /// <returns>The new message.</returns>
        Message MakeMessage();
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
    /// Additional internal methods to drive the execution of the dialog context.
    /// </summary>
    public interface IDialogContextInternal : IDialogContext, IPostToBot
    {
        /// <summary>
        /// Poll the dialog stack for any work to be done.
        /// </summary>
        /// <returns>A task that represents the poll operation.</returns>
        Task PollAsync();
    }
}