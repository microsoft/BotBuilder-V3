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
using Microsoft.Bot.Builder.Scorables.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Builder.Scorables
{
    /// <summary>
    /// Fluent methods related to <see cref="IScorable{IResolver, Score}"/>.
    /// </summary>
    public static partial class Actions
    {
        private static IScorable<IResolver, Binding> Bind(Delegate lambda)
        {
            return new DelegateScorable(lambda);
        }

        public static IScorable<IResolver, Binding> Bind<R>(Func<R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Binding> Bind<T1, R>(Func<T1, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Binding> Bind<T1, T2, R>(Func<T1, T2, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Binding> Bind<T1, T2, T3, R>(Func<T1, T2, T3, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Binding> Bind<T1, T2, T3, T4, R>(Func<T1, T2, T3, T4, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Binding> Bind<T1, T2, T3, T4, T5, R>(Func<T1, T2, T3, T4, T5, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Binding> Bind<T1, T2, T3, T4, T5, T6, R>(Func<T1, T2, T3, T4, T5, T6, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Binding> Bind<T1, T2, T3, T4, T5, T6, T7, R>(Func<T1, T2, T3, T4, T5, T6, T7, R> method)
        {
            return Bind((Delegate)method);
        }

        public static IScorable<IResolver, Match> When<InnerScore>(this IScorable<IResolver, InnerScore> scorable, Regex regex)
        {
            return new RegexMatchScorable<object, InnerScore>(regex, scorable);
        }

        public static IScorable<IResolver, IntentRecommendation> When<InnerScore>(this IScorable<IResolver, InnerScore> scorable, ILuisModel model, LuisIntentAttribute intent, ILuisService service = null)
        {
            service = service ?? new LuisService(model);
            return new LuisIntentScorable<object, InnerScore>(service, model, intent, scorable);
        }

        public static IScorable<IResolver, double> Normalize(this IScorable<IResolver, Binding> scorable)
        {
            return scorable.SelectScore((r, b) => 1.0);
        }

        public static IScorable<IResolver, double> Normalize(this IScorable<IResolver, Match> scorable)
        {
            return scorable.SelectScore((r, m) => RegexMatchScorable.ScoreFor(m));
        }

        public static IScorable<IResolver, double> Normalize(this IScorable<IResolver, IntentRecommendation> scorable)
        {
            return scorable.SelectScore((r, i) => i.Score.GetValueOrDefault());
        }
    }
}