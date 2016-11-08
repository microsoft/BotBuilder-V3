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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    /// <summary>
    /// Fold an aggregation of scorables to produce a winning scorable.
    /// </summary>
    [Serializable]
    public class FoldScorable<Item, Score> : ScorableBase<Item, IReadOnlyList<FoldScorable<Item, Score>.State>, Score>
    {
        protected readonly IComparer<Score> comparer;
        protected readonly IEnumerable<IScorable<Item, Score>> scorables;
        public FoldScorable(IComparer<Score> comparer, IEnumerable<IScorable<Item, Score>> scorables)
        {
            SetField.NotNull(out this.comparer, nameof(comparer), comparer);
            SetField.NotNull(out this.scorables, nameof(scorables), scorables);
        }

        protected virtual bool OnFold(IScorable<Item, Score> scorable, Item item, object state, Score score)
        {
            return true;
        }

        /// <summary>
        /// Per-scorable opaque state used during scoring process.
        /// </summary>
        public struct State
        {
            public readonly IScorable<Item, Score> scorable;
            public readonly object state;
            public State(IScorable<Item, Score> scorable, object state)
            {
                this.scorable = scorable;
                this.state = state;
            }
        }

        protected override async Task<IReadOnlyList<State>> PrepareAsync(Item item, CancellationToken token)
        {
            var states = new List<State>();

            foreach (var scorable in this.scorables)
            {
                var state = await scorable.PrepareAsync(item, token);
                states.Add(new State(scorable, state));
                if (scorable.HasScore(item, state))
                {
                    var score = scorable.GetScore(item, state);
                    if (!OnFold(scorable, item, state, score))
                    {
                        break;
                    }
                }
            }

            states.Sort((one, two) =>
            {
                var oneHasScore = one.scorable.HasScore(item, one.state);
                var twoHasScore = two.scorable.HasScore(item, two.state);
                if (oneHasScore && twoHasScore)
                {
                    var oneScore = one.scorable.GetScore(item, one.state);
                    var twoScore = two.scorable.GetScore(item, two.state);
                    return this.comparer.Compare(twoScore, oneScore);
                }
                else if (oneHasScore)
                {
                    return -1;
                }
                else if (twoHasScore)
                {
                    return +1;
                }
                else
                {
                    return 0;
                }
            });

            return states;
        }
        protected override bool HasScore(Item item, IReadOnlyList<State> states)
        {
            if (states.Count > 0)
            {
                var state = states[0];
                return state.scorable.HasScore(item, state.state); 
            }

            return false;
        }
        protected override Score GetScore(Item item, IReadOnlyList<State> states)
        {
            var state = states[0];
            return state.scorable.GetScore(item, state.state);
        }
        protected override Task PostAsync(Item item, IReadOnlyList<State> states, CancellationToken token)
        {
            try
            {
                var state = states[0];
                return state.scorable.PostAsync(item, state.state, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }
        protected override async Task DoneAsync(Item item, IReadOnlyList<State> states, CancellationToken token)
        {
            foreach (var state in states)
            {
                await state.scorable.DoneAsync(item, state.state, token);
            }
        }
    }
}
