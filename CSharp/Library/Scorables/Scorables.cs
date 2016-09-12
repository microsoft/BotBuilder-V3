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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    public static partial class Scorables
    {
        /// <summary>
        /// Invoke the scorable calling protocol against a single scorable.
        /// </summary>
        public static async Task<bool> TryPostAsync<Item, Score>(this IScorable<Item, Score> scorable, Item item, CancellationToken token)
        {
            var state = await scorable.PrepareAsync(item, token);
            if (scorable.HasScore(item, state))
            {
                var score = scorable.GetScore(item, state);
                await scorable.PostAsync(item, state, token);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Project the score of a scorable using a lambda expression.
        /// </summary>
        public static IScorable<Item, TargetScore> Select<Item, SourceScore, TargetScore>(this IScorable<Item, SourceScore> scorable, Func<Item, SourceScore, TargetScore> selector)
        {
            return new SelectScorable<Item, SourceScore, TargetScore>(scorable, selector);
        }

        /// <summary>
        /// Select the first scorable that produces a score.
        /// </summary>
        public static IScorable<Item, Score> First<Item, Score>(this IEnumerable<IScorable<Item, Score>> scorables)
        {
            return new FirstScorable<Item, Score>(scorables);
        }

        /// <summary>
        /// Select a single winning scorable from an enumeration of scorables using a score comparer.
        /// </summary>
        public static IScorable<Item, Score> Fold<Item, Score>(this IEnumerable<IScorable<Item, Score>> scorables, IComparer<Score> comparer)
        {
            return new FoldScorable<Item, Score>(comparer, scorables);
        }
    }

    public sealed class SelectScorable<Item, SourceScore, TargetScore> : DelegatingScorable<Item, SourceScore>, IScorable<Item, TargetScore>
    {
        private readonly Func<Item, SourceScore, TargetScore> selector;
        public SelectScorable(IScorable<Item, SourceScore> scorable, Func<Item, SourceScore, TargetScore> selector)
            : base(scorable)
        {
            SetField.NotNull(out this.selector, nameof(selector), selector);
        }

        TargetScore IScorable<Item, TargetScore>.GetScore(Item item, object state)
        {
            IScorable<Item, SourceScore> source = this;
            var sourceScore = source.GetScore(item, state);
            var targetScore = this.selector(item, sourceScore);
            return targetScore;
        }
    }

    public sealed class FirstScorable<Item, Score> : FoldScorable<Item, Score>
    {
        public FirstScorable(IEnumerable<IScorable<Item, Score>> scorables)
            : base(Comparer<Score>.Default, scorables)
        {
        }
        protected override bool OnFold(IScorable<Item, Score> scorable, Item item, object state, Score score)
        {
            return false;
        }
    }

    public sealed class TraitsScorable<Item, Score> : FoldScorable<Item, Score>
    {
        private readonly ITraits<Score> traits;
        public TraitsScorable(ITraits<Score> traits, IComparer<Score> comparer, IEnumerable<IScorable<Item, Score>> scorables)
            : base(comparer, scorables)
        {
            SetField.NotNull(out this.traits, nameof(traits), traits);
        }
        protected override bool OnFold(IScorable<Item, Score> scorable, Item item, object state, Score score)
        {
            if (this.comparer.Compare(score, this.traits.Minimum) < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score));
            }

            var maximum = this.comparer.Compare(score, this.traits.Maximum);
            if (maximum > 0)
            {
                throw new ArgumentOutOfRangeException(nameof(score));
            }
            else if (maximum == 0)
            {
                return false;
            }

            return true;
        }
    }
}