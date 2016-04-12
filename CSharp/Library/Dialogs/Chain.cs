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

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

        public static IDialog<Connector.Message> PostToChain()
        {
            return new PostToChainDialog();
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
        /// When the antecedent <see cref="IDialog{IDialog{T}}"/> has completed, unwrap the result into a new <see cref="IDialog{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the antecedent dialog.</typeparam>
        /// <param name="antecedent">The antecedent dialog <see cref="IDialog{IDialog{T}}"/>.</param>
        /// <returns>The result <see cref="IDialog{T}"/>.</returns>
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
        /// Loop the <see cref="IDialog"/> forever.
        /// </summary>
        /// <param name="antecedent">The antecedent <see cref="IDialog"/>.</param>
        /// <returns>The looping dialog.</returns>
        public static IDialog<T> Loop<T>(this IDialog<T> antecedent)
        {
            return new LoopDialog<T>(antecedent);
        }

        public interface ICase<T, R>
        {
            Func<T, bool> Condition { get; }
            Func<IBotContext, T, R> Selector { get; }
        }

        [Serializable]
        public class Case<T, R> : ICase<T, R>
        {
            public Func<T, bool> Condition { get; protected set; }
            public Func<IBotContext, T, R> Selector { get; protected set; }

            protected Case()
            {
            }
            public Case(Func<T, bool> condition, Func<IBotContext, T, R> selector)
            {
                SetField.ThrowOnNullField(nameof(condition), condition);
                this.Condition = condition;
                SetField.ThrowOnNullField(nameof(selector), selector);
                this.Selector = selector;
            }
        }

        [Serializable]
        public sealed class RegexCase<R> : Case<string, R>
        {
            private readonly Regex Regex;

            public RegexCase(Regex regex, Func<IBotContext, string, R> selector)
            {
                SetField.ThrowOnNullField(nameof(selector), selector);
                this.Selector = selector;
                SetField.NotNull(out this.Regex, nameof(regex), regex);
                this.Condition = this.IsMatch;
            }

            private bool IsMatch(string text)
            {
                return this.Regex.Match(text).Success;
            }
        }

        [Serializable]
        public sealed class DefaultCase<T, R> : Case<T, R>
        {
            public DefaultCase(Func<IBotContext, T, R> selector)
                : base(obj => true, selector)
            {
            }
        }
        public static IDialog<R> Switch<T, R>(this IDialog<T> antecedent, params ICase<T, R>[] cases)
        {
            return new SwitchDialog<T, R>(antecedent, cases);
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
                if (item is Connector.Message)
                {
                    var msg = item as Connector.Message;
                    await context.PostAsync(msg);
                }
                else
                {
                    await context.PostAsync(item.ToString());
                }

                context.Done<T>(await result);
            }
        }

        [Serializable]
        private sealed class PostToChainDialog : IDialog<Connector.Message>
        {
            public async Task StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceivedAsync);
            }

            public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Connector.Message> argument)
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
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> ignored)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
        }


        [Serializable]
        private sealed class SwitchDialog<T, R> : IDialog<R>
        {
            public readonly IDialog<T> Antecedent;
            public readonly IList<ICase<T, R>> Cases;
            public SwitchDialog(IDialog<T> antecedent, IList<ICase<T, R>> cases)
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

        [Serializable]
        public class ValueWrapperDialog<T> : IDialog<T>
        {
            public readonly T Value;

            public ValueWrapperDialog(T value)
            {
                this.Value = value;
            }

            public async Task StartAsync(IDialogContext context)
            {
                context.Done(Value);
            }
        }
    }
}
