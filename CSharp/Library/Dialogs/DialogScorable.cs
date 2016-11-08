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
using System.Linq;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Internals.Scorables;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// Scorable for Dialog module.
    /// </summary>
    [Serializable]
    public sealed class DialogScorable : DelegatingScorable<IActivity, double>
    {
        public static IEnumerable<IScorable<IActivity, double>> Make(
            IDialogStack stack,
            IEnumerable<IScorable<IActivity, double>> fromActivity,
            IEnumerable<IScorable<IResolver, double>> fromResolver,
            Func<IActivity, IResolver> makeResolver)
        {
            // first, let's go through stack frames
            var targets = stack.Frames.Select(f => f.Target);
            foreach (var target in targets)
            {
                var activity = target as IScorable<IActivity, double>;
                if (activity != null)
                {
                    yield return activity;
                }

                var resolver = target as IScorable<IResolver, double>;
                if (resolver != null)
                {
                    yield return resolver.SelectItem(makeResolver);
                }
            }

            // then global scorables "on the side"
            foreach (var activity in fromActivity)
            {
                yield return activity;
            }

            foreach (var resolver in fromResolver)
            {
                yield return resolver.SelectItem(makeResolver);
            }
        }

        public static IScorable<IActivity, double> Make(
            IDialogStack stack,
            IEnumerable<IScorable<IActivity, double>> fromActivity,
            IEnumerable<IScorable<IResolver, double>> fromResolver,
            Func<IActivity, IResolver> makeResolver,
            ITraits<double> traits,
            IComparer<double> comparer)
        {
            // since the stack of scorables changes over time, this should be lazy
            var lazyScorables = Make(stack, fromActivity, fromResolver, makeResolver);
            var scorable = new TraitsScorable<IActivity, double>(traits, comparer, lazyScorables);
            return scorable;
        }

        public DialogScorable(
            IDialogStack stack,
            IEnumerable<IScorable<IActivity, double>> fromActivity,
            IEnumerable<IScorable<IResolver, double>> fromResolver,
            Func<IActivity, IResolver> makeResolver,
            ITraits<double> traits,
            IComparer<double> comparer)
            : base(Make(stack, fromActivity, fromResolver, makeResolver, traits, comparer))
        {
        }
    }
}
