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

using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A fluent, chainable interface for IDialogs.
    /// </summary>
    public static partial class Chain
    {
        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, execute this continuation method to construct the next <see cref="IDialog{R}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R">The type of the next dialog.</typeparam>
        /// <param name="context">The bot context.</param>
        /// <param name="item">The result of the previous <see cref="IDialog{T}"/>.</param>
        /// <returns>A task that represents the next <see cref="IDialog{R}"/>.</returns>
        public delegate Task<IDialog<R>> Continutation<in T, R>(IBotContext context, IAwaitable<T> item);

        /// <summary>
        /// Construct a <see cref="IDialog{T}"/> that will make a new copy of another <see cref="IDialog{T}"/> when started.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="MakeDialog">The dialog factory method.</param>
        /// <returns>The new dialog.</returns>
        public static IDialog<T> From<T>(Func<IDialog<T>> MakeDialog)
        {
            return new FromDialog<T>(MakeDialog);
        }

        /// <summary>
        /// Execute a side-effect after a <see cref="IDialog{T}"/> completes.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <param name="callback">The callback method.</param>
        /// <returns>The antecedent dialog.</returns>
        public static IDialog<T> Do<T>(this IDialog<T> antecedent, Func<IBotContext, IAwaitable<T>, Task> callback)
        {
            return new DoDialog<T>(antecedent, callback);
        }

        /// <summary>
        /// Post to the user the result of a <see cref="IDialog{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <returns>The antecedent dialog.</returns>
        public static IDialog<T> PostToUser<T>(this IDialog<T> antecedent)
        {
            return new PostToUserDialog<T>(antecedent);
        }

        /// <summary>
        /// Post to the chain the message to the bot after the antecedent completes.
        /// </summary>
        /// <typeparam name="T">The type of the dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <returns>The dialog representing the message sent to the bot.</returns>
        public static IDialog<Connector.IMessageActivity> WaitToBot<T>(this IDialog<T> antecedent)
        {
            return new WaitToBotDialog<T>(antecedent);
        }

        /// <summary>
        /// Post the message from the user to Chain.
        /// </summary>
        /// <remarks>
        /// The returned <see cref="IDialog{T}"/> can be used as the root dialog for a chain.
        /// </remarks>
        /// <returns> The dialog that dispatches the incoming message from the user to chain.</returns>
        public static IDialog<Connector.IMessageActivity> PostToChain()
        {
            return Chain.Return(string.Empty).WaitToBot();
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, execute the continuation to produce the next <see cref="IDialog{R}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R">The type of the next dialog.</typeparam>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <param name="continuation">The continuation to produce the next <see cref="IDialog{R}"/>.</param>
        /// <returns>The next <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> ContinueWith<T, R>(this IDialog<T> antecedent, Continutation<T, R> continuation)
        {
            return new ContinueWithDialog<T, R>(antecedent, continuation);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, project the result into a new <see cref="IDialog{R}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R">The type of the projected dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="selector">The projection function from <typeparamref name="T"/> to <typeparamref name="R"/>.</param>
        /// <returns>The result <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> Select<T, R>(this IDialog<T> antecedent, Func<T, R> selector)
        {
            return new SelectDialog<T, R>(antecedent, selector);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, evaluate the predicate and decide whether to continue.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="predicate">The predicate to decide whether to continue the chain.</param>
        /// <returns>The result from the antecedent <see cref="IDialog{T}"/> or its cancellation, wrapped in a <see cref="IDialog{T}"/>.</returns>
        public static IDialog<T> Where<T>(this IDialog<T> antecedent, Func<T, bool> predicate)
        {
            return new WhereDialog<T>(antecedent, predicate);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> where T is <see cref="IDialog{T}"/> completes, unwrap the result into a new <see cref="IDialog{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/> where T is <see cref="IDialog{T}"/>.</param>
        /// <returns>An <see cref="IDialog{T}"/>.</returns>
        public static IDialog<T> Unwrap<T>(this IDialog<IDialog<T>> antecedent)
        {
            return new UnwrapDialog<T>(antecedent);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, execute the next <see cref="IDialog{C}"/>, and use the projection to combine the results.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <typeparam name="C">The type of the intermediate dialog.</typeparam>
        /// <typeparam name="R">The type of the projected dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="function">The factory method to create the next dialog <see cref="IDialog{C}"/>.</param>
        /// <param name="projection">The projection function for the combination of the two dialogs.</param>
        /// <returns>The result <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> SelectMany<T, C, R>(this IDialog<T> antecedent, Func<T, IDialog<C>> function, Func<T, C, R> projection)
        {
            return new SelectManyDialog<T, C, R>(antecedent, function, projection);
        }

        /// <summary>
        /// Loop the <see cref="IDialog{T}"/> forever.
        /// </summary>
        /// <param name="antecedent">The antecedent <see cref="IDialog{T}"/>.</param>
        /// <returns>The looping dialog.</returns>
        public static IDialog<T> Loop<T>(this IDialog<T> antecedent)
        {
            return new LoopDialog<T>(antecedent);
        }

        /// <summary>
        /// Call the voided <see cref="IDialog{T}"/>, ignore the result, then restart the original dialog wait.
        /// </summary>
        /// <typeparam name="T">The type of the voided dialog.</typeparam>
        /// <typeparam name="R">The type of the original dialog wait.</typeparam>
        /// <param name="antecedent">The voided dialog.</param>
        /// <returns>The dialog that produces the item to satisfy the original wait.</returns>
        public static IDialog<R> Void<T, R>(this IDialog<T> antecedent)
        {
            return new VoidDialog<T, R>(antecedent);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, stop the propagation of an exception of <typeparamref name="E"/>.
        /// </summary>
        /// <typeparam name="T">The type returned by the antecedent dialog.</typeparam>
        /// <typeparam name="E">The type of exception to swallow.</typeparam>
        /// <param name="antecedent"> The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <returns>The default value of <typeparamref name="T"/> if there is an exception of type <typeparamref name="E"/>.</returns>
        public static IDialog<T> DefaultIfException<T, E>(this IDialog<T> antecedent) where E : Exception
        {
            return new DefaultIfExceptionDialog<T, E>(antecedent);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, stop the propagation of Exception.
        /// </summary>
        /// <typeparam name="T">The type returned by the antecedent dialog.</typeparam>
        /// <param name="antecedent"> The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <returns>The default value of <typeparamref name="T"/> if there is an Exception.</returns>
        public static IDialog<T> DefaultIfException<T>(this IDialog<T> antecedent)
        {
            return new DefaultIfExceptionDialog<T, Exception>(antecedent);
        }

        /// <summary>
        /// When the antecedent <see cref="IDialog{T}"/> has completed, go through each <see cref="ICase{T, R}"/> 
        /// and run the <see cref="ContextualSelector{T, R}"/>" of the first <see cref="ICase{T, R}"/> that 
        /// the returned value by the antecedent dialog satisfies.
        /// </summary>
        /// <typeparam name="T"> The type of the antecedent dialog.</typeparam>
        /// <typeparam name="R"> The type of the Dialog returned by <see cref="ContextualSelector{T, R}"/></typeparam>
        /// <param name="antecedent"> The antecedent dialog <see cref="IDialog{T}"/>.</param>
        /// <param name="cases"> Cases for the switch</param>
        /// <returns>The result <see cref="IDialog{R}"/>.</returns>
        public static IDialog<R> Switch<T, R>(this IDialog<T> antecedent, params ICase<T, R>[] cases)
        {
            return new SwitchDialog<T, R>(antecedent, cases);
        }

        /// <summary>
        /// Creates a <see cref="IDialog{T}"/> that returns a value.
        /// </summary>
        /// <remarks>
        /// The type of the value should be serializable.
        /// </remarks>
        /// <typeparam name="T"> Type of the value.</typeparam>
        /// <param name="item"> The value to be wrapped.</param>
        /// <returns> The <see cref="IDialog{T}"/> that wraps the value.</returns>
        public static IDialog<T> Return<T>(T item)
        {
            return new ReturnDialog<T>(item);
        }

        /// <summary>
        /// Fold items from an enumeration of dialogs.
        /// </summary>
        /// <typeparam name="T"> The type of the dialogs in the enumeration produced by the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog that produces an enumeration of <see cref="IDialog{T}"/>.</param>
        /// <param name="folder">The accumulator for the dialog enumeration.</param>
        /// <returns>The accumulated result.</returns>
        public static IDialog<T> Fold<T>(this IDialog<IEnumerable<IDialog<T>>> antecedent, Func<T, T, T> folder)
        {
            return new FoldDialog<T>(antecedent, folder);
        }

        /// <summary>
        /// Constructs a case. 
        /// </summary>
        /// <typeparam name="T"> The type of incoming value to case.</typeparam>
        /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
        /// <param name="condition"> The condition of the case.</param>
        /// <param name="selector"> The contextual selector of the case.</param>
        /// <returns></returns>
        public static ICase<T, R> Case<T, R>(Func<T, bool> condition, ContextualSelector<T, R> selector)
        {
            return new Case<T, R>(condition, selector);
        }

        /// <summary>
        /// Constructs a case based on a regular expression.
        /// </summary>
        /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
        /// <param name="regex"> The regex for condition.</param>
        /// <param name="selector"> The contextual selector for the case.</param>
        /// <returns>The case.</returns>
        public static ICase<string, R> Case<R>(Regex regex, ContextualSelector<string, R> selector)
        {
            return new RegexCase<R>(regex, selector);
        }

        /// <summary>
        /// Constructs a case to use as the default.
        /// </summary>
        /// <typeparam name="T"> The type of incoming value to case.</typeparam>
        /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
        /// <param name="selector"> The contextual selector of the case.</param>
        /// <returns>The case.</returns>
        public static ICase<T, R> Default<T, R>(ContextualSelector<T, R> selector)
        {
            return new DefaultCase<T, R>(selector);
        }

        [Serializable]
        private sealed class FromDialog<T> : IDialog<T>
        {
            public readonly Func<IDialog<T>> MakeDialog;
            public FromDialog(Func<IDialog<T>> MakeDialog)
            {
                SetField.NotNull(out this.MakeDialog, nameof(MakeDialog), MakeDialog);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                var dialog = this.MakeDialog();
                context.Call<T>(dialog, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                context.Done<T>(await result);
            }
        }

        [Serializable]
        private sealed class DoDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<IBotContext, IAwaitable<T>, Task> Action;
            public DoDialog(IDialog<T> antecedent, Func<IBotContext, IAwaitable<T>, Task> Action)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Action, nameof(Action), Action);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                await this.Action(context, result);
                context.Done<T>(await result);
            }
        }

        [Serializable]
        private sealed class PostToUserDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public PostToUserDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var item = await result;
                await context.PostAsync(item.ToString());
                context.Done<T>(item);
            }
        }

        [Serializable]
        private sealed class WaitToBotDialog<T> : IDialog<Connector.IMessageActivity>
        {
            public readonly IDialog<T> Antecedent;
            public WaitToBotDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            public async Task StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var item = await result;
                context.Wait(MessageReceivedAsync);
            }
            public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Connector.IMessageActivity> argument)
            {
                context.Done(await argument);
            }
        }

        [Serializable]
        private sealed class ContinueWithDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Continutation<T, R> Continuation;

            public ContinueWithDialog(IDialog<T> antecedent, Continutation<T, R> continuation)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Continuation, nameof(continuation), continuation);
            }

            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }

            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var next = await this.Continuation(context, result);
                context.Call<R>(next, DoneAsync);
            }

            private async Task DoneAsync(IDialogContext context, IAwaitable<R> result)
            {
                context.Done(await result);
            }
        }

        [Serializable]
        private sealed class SelectDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<T, R> Selector;
            public SelectDialog(IDialog<T> antecedent, Func<T, R> selector)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Selector, nameof(selector), selector);
            }
            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, AfterAntecedent);
            }
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                var itemR = this.Selector(itemT);
                context.Done(itemR);
            }
        }

        /// <summary>
        /// The exception that is thrown when the where is canceled.
        /// </summary>
        [Serializable]
        public sealed class WhereCanceledException : OperationCanceledException
        {
            /// <summary>
            /// Construct the exception.
            /// </summary>
            public WhereCanceledException()
            {
            }

            /// <summary>
            /// This is the serialization constructor.
            /// </summary>
            /// <param name="info">The serialization info.</param>
            /// <param name="context">The streaming context.</param>
            private WhereCanceledException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }

        private sealed class WhereDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<T, bool> Predicate;
            public WhereDialog(IDialog<T> antecedent, Func<T, bool> predicate)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Predicate, nameof(predicate), predicate);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, AfterAntecedent);
            }
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                var itemR = this.Predicate(itemT);
                if (itemR)
                {
                    context.Done(itemT);
                }
                else
                {
                    throw new WhereCanceledException();
                }
            }
        }

        [Serializable]
        private sealed class UnwrapDialog<T> : IDialog<T>
        {
            public readonly IDialog<IDialog<T>> Antecedent;
            public UnwrapDialog(IDialog<IDialog<T>> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<IDialog<T>>(this.Antecedent, AfterAntecedent);
            }
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<IDialog<T>> result)
            {
                var dialogT = await result;
                context.Call<T>(dialogT, AfterDialog);
            }
            private async Task AfterDialog(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                context.Done(itemT);
            }
        }

        // http://blogs.msdn.com/b/pfxteam/archive/2013/04/03/tasks-monads-and-linq.aspx
        [Serializable]
        private sealed class SelectManyDialog<T, C, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly Func<T, IDialog<C>> Function;
            public readonly Func<T, C, R> Projection;
            public SelectManyDialog(IDialog<T> antecedent, Func<T, IDialog<C>> function, Func<T, C, R> projection)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Function, nameof(function), function);
                SetField.NotNull(out this.Projection, nameof(projection), projection);
            }
            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, AfterAntecedent);
            }
            private T itemT;
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                this.itemT = await result;
                var dialog = this.Function(this.itemT);
                context.Call<C>(dialog, AfterFunction);
            }
            private async Task AfterFunction(IDialogContext context, IAwaitable<C> result)
            {
                var itemC = await result;
                var itemR = this.Projection(itemT, itemC);
                context.Done(itemR);
            }
        }

        [Serializable]
        private sealed class LoopDialog<T> : IDialog<T>
        {
            public readonly IDialog<T> Antecedent;
            public LoopDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                await result;
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
        }

        [Serializable]
        private sealed class VoidDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public VoidDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                var ignore = await result;
                context.Wait<R>(ItemReceived);
            }
            private async Task ItemReceived(IDialogContext context, IAwaitable<R> result)
            {
                var item = await result;
                context.Done(item);
            }
        }

        [Serializable]
        private sealed class DefaultIfExceptionDialog<T, E> : IDialog<T> where E: Exception
        {
            public readonly IDialog<T> Antecedent;
            public DefaultIfExceptionDialog(IDialog<T> antecedent)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                try
                {
                    context.Done(await result);
                }
                catch (E)
                {
                    context.Done(default(T));
                }
            }
        }

        [Serializable]
        private sealed class SwitchDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly IReadOnlyList<ICase<T, R>> Cases;
            public SwitchDialog(IDialog<T> antecedent, IReadOnlyList<ICase<T, R>> cases)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Cases, nameof(cases), cases);
            }

            async Task IDialog<R>.StartAsync(IDialogContext context)
            {
                context.Call<T>(Antecedent, AfterAntecedent);
            }

            private async Task AfterAntecedent(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                R itemR = default(R);
                foreach (var condition in this.Cases)
                {
                    if (condition.Condition(itemT))
                    {
                        itemR = condition.Selector(context, itemT);
                        break;
                    }
                }
                context.Done(itemR);
            }
        }

        /// <summary>
        /// A Dialog that wraps a value of type T.
        /// </summary>
        /// <remarks>
        /// The type of the value should be serializable.
        /// </remarks>
        /// <typeparam name="T">The result type of the Dialog. </typeparam>
        [Serializable]
        private sealed class ReturnDialog<T> : IDialog<T>
        {
            public readonly T Value;

            public ReturnDialog(T value)
            {
                this.Value = value;
            }

            public async Task StartAsync(IDialogContext context)
            {
                context.Done(Value);
            }
        }

        [Serializable]
        private sealed class FoldDialog<T> : IDialog<T>
        {
            public readonly IDialog<IEnumerable<IDialog<T>>> Antecedent;
            public readonly Func<T, T, T> Folder;
            public FoldDialog(IDialog<IEnumerable<IDialog<T>>> antecedent, Func<T, T, T> folder)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Folder, nameof(folder), folder);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call(this.Antecedent, this.AfterAntecedent);
            }
            private IReadOnlyList<IDialog<T>> items;
            private async Task AfterAntecedent(IDialogContext context, IAwaitable<IEnumerable<IDialog<T>>> result)
            {
                this.items = (await result).ToArray();
                await Iterate(context);
            }
            private int index = 0;
            private T folded = default(T);
            private async Task Iterate(IDialogContext context)
            {
                if (this.index < this.items.Count)
                {
                    var child = this.items[this.index];
                    context.Call(child, AfterItem);
                }
                else
                {
                    context.Done(this.folded);
                }
            }
            private async Task AfterItem(IDialogContext context, IAwaitable<T> result)
            {
                var itemT = await result;
                if (this.index == 0)
                {
                    this.folded = itemT;
                }
                else
                {
                    this.folded = this.Folder(this.folded, itemT);
                }

                ++this.index;

                await this.Iterate(context);
            }
        }
    }

    /// <summary>
    /// The contextual selector function.
    /// </summary>
    /// <typeparam name="T"> The type of value passed to selector.</typeparam>
    /// <typeparam name="R"> The returned type of the selector.</typeparam>
    /// <param name="context"> <see cref="IBotContext"/> passed to selector.</param>
    /// <param name="item"> The value passed to selector.</param>
    /// <returns> The value returned by selector.</returns>
    public delegate R ContextualSelector<in T, R>(IBotContext context, T item);

    /// <summary>
    /// The interface for cases evaluated by switch.
    /// </summary>
    /// <typeparam name="T"> The type of incoming value to case.</typeparam>
    /// <typeparam name="R"> The type of the object returned by selector.</typeparam>
    public interface ICase<in T, R>
    {
        /// <summary>
        /// The condition field of the case.
        /// </summary>
        Func<T, bool> Condition { get; }
        /// <summary>
        /// The selector that will be invoked if condition is met.
        /// </summary>
        ContextualSelector<T, R> Selector { get; }
    }

    /// <summary>
    /// The default implementation of <see cref="ICase{T, R}"/>.
    /// </summary>
    [Serializable]
    public class Case<T, R> : ICase<T, R>
    {
        public Func<T, bool> Condition { get; protected set; }
        public ContextualSelector<T, R> Selector { get; protected set; }

        protected Case()
        {
        }

        /// <summary>
        /// Constructs a case. 
        /// </summary>
        /// <param name="condition"> The condition of the case.</param>
        /// <param name="selector"> The contextual selector of the case.</param>
        public Case(Func<T, bool> condition, ContextualSelector<T, R> selector)
        {
            SetField.CheckNull(nameof(condition), condition);
            this.Condition = condition;
            SetField.CheckNull(nameof(selector), selector);
            this.Selector = selector;
        }
    }

    /// <summary>
    /// The regex case for switch.
    /// </summary>
    /// <remarks>
    /// The condition will be true if the regex matches the text.
    /// </remarks>
    [Serializable]
    public sealed class RegexCase<R> : Case<string, R>
    {
        private readonly Regex Regex;

        /// <summary>
        /// Constructs a case based on a regular expression.
        /// </summary>
        /// <param name="regex"> The regex for condition.</param>
        /// <param name="selector"> The contextual selector for the case.</param>
        public RegexCase(Regex regex, ContextualSelector<string, R> selector)
        {
            SetField.CheckNull(nameof(selector), selector);
            this.Selector = selector;
            SetField.NotNull(out this.Regex, nameof(regex), regex);
            this.Condition = this.IsMatch;
        }

        private bool IsMatch(string text)
        {
            return this.Regex.Match(text).Success;
        }
    }

    /// <summary>
    /// The default case for switch. <see cref="ICase{T, R}"/>
    /// </summary>
    [Serializable]
    public sealed class DefaultCase<T, R> : Case<T, R>
    {
        /// <summary>
        /// Constructs the default case for switch.
        /// </summary>
        /// <param name="selector"> The contextual selector that will be called in default case.</param>
        public DefaultCase(ContextualSelector<T, R> selector)
            : base(obj => true, selector)
        {
        }
    }
}
