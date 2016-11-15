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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public sealed class ScoringDialogTask<Score> : IPostToBot
    {
        private readonly IPostToBot inner;
        private readonly IScorable<IActivity, Score> scorable;
        public ScoringDialogTask(IPostToBot inner, IScorable<IActivity, Score> scorable)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
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
}

namespace Microsoft.Bot.Builder.Scorables
{
    public static partial class Scorable
    {
        public static IScorable<IResolver, Binding> For(Delegate lambda)
        {
            return new DelegateScorable(lambda);
        }

        public static IScorable<IResolver, Binding> For<R>(Func<R> method)
        {
            return For((Delegate)method);
        }

        public static IScorable<IResolver, Binding> For<T1, R>(Func<T1, R> method)
        {
            return For((Delegate)method);
        }

        public static IScorable<IResolver, Binding> For<T1, T2, R>(Func<T1, T2, R> method)
        {
            return For((Delegate)method);
        }

        public static IScorable<IResolver, Binding> For<T1, T2, T3, R>(Func<T1, T2, T3, R> method)
        {
            return For((Delegate)method);
        }

        public static IScorable<IResolver, Binding> For<T1, T2, T3, T4, R>(Func<T1, T2, T3, T4, R> method)
        {
            return For((Delegate)method);
        }

        public static IScorable<IResolver, Binding> For<T1, T2, T3, T4, T5, R>(Func<T1, T2, T3, T4, T5, R> method)
        {
            return For((Delegate)method);
        }

        public static IScorable<IResolver, double> When(this IScorable<IResolver, Binding> scorable)
        {
            var normalized = scorable.SelectScore((r, b) => 1.0);
            return normalized;
        }

        public static IScorable<IResolver, double> When(this IScorable<IResolver, Binding> scorable, Regex regex)
        {
            var resolved = new RegexMatchScorable<Binding, Binding>(regex, scorable);
            var normalized = resolved.SelectScore((r, m) => RegexMatchScorable.ScoreFor(m));
            return normalized;
        }

        public static IScorable<IResolver, double> When(this IScorable<IResolver, Binding> scorable, ILuisModel model, LuisIntentAttribute intent, ILuisService service = null)
        {
            service = service ?? new LuisService(model);
            var resolved = new LuisIntentScorable<Binding, Binding>(service, model, intent, scorable);
            var normalized = resolved.SelectScore((r, i) => i.Score ?? 0);
            return normalized;
        }
    }
}
