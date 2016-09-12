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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Internals.Scorables;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class ScoringDialogTask<Score> : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IDialogStack stack;
        private readonly IScorable<IActivity, Score> scorable;
        public ScoringDialogTask(IPostToBot inner, IDialogStack stack, IScorable<IActivity, Score> scorable)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.scorable, nameof(scorable), scorable);
        }

        async Task IPostToBot.PostAsync<T>(T item, CancellationToken token)
        {
            var activity = item as IActivity;
            if (activity != null)
            {
                if (await this.scorable.TryPostAsync(activity, token))
                {
                    return;
                }
            }

            await this.inner.PostAsync<T>(item, token);
        }
    }


    public partial class Extensions
    {
        public static IDialog<T> WithScorable<T, Item, Score>(this IDialog<T> antecedent, IScorable<Item, Score> scorable)
        {
            return new WithScorableDialog<T, Item, Score>(antecedent, scorable);
        }

        [Serializable]
        private sealed class WithScorableDialog<T, Item, Score> : IDialog<T>, IScorable<Item, Score>
        {
            public readonly IDialog<T> Antecedent;
            public readonly IScorable<Item, Score> Scorable;
            public WithScorableDialog(IDialog<T> antecedent, IScorable<Item, Score> scorable)
            {
                SetField.NotNull(out this.Antecedent, nameof(antecedent), antecedent);
                SetField.NotNull(out this.Scorable, nameof(scorable), scorable);
            }
            async Task IDialog<T>.StartAsync(IDialogContext context)
            {
                context.Call<T>(this.Antecedent, ResumeAsync);
            }
            private async Task ResumeAsync(IDialogContext context, IAwaitable<T> result)
            {
                context.Done(await result);
            }
            async Task<object> IScorable<Item, Score>.PrepareAsync(Item item, CancellationToken token)
            {
                return await this.Scorable.PrepareAsync(item, token);
            }
            bool IScorable<Item, Score>.HasScore(Item item, object state)
            {
                return this.Scorable.HasScore(item, state);
            }
            Score IScorable<Item, Score>.GetScore(Item item, object state)
            {
                return this.Scorable.GetScore(item, state);
            }
            async Task IScorable<Item, Score>.PostAsync(Item item, object state, CancellationToken token)
            {
                await this.Scorable.PostAsync(item, state, token);
            }
        }
    }
}
