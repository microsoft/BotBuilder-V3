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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    /// <summary>
    /// Allow for static type checking of opaque state for convenience of scorable implementations.
    /// </summary>
    public abstract class ScorableBase<Item, State, Score> : IScorable<Item, Score>
    {
        public abstract Task<State> PrepareAsync(Item item, CancellationToken token);
        public abstract bool HasScore(Item item, State state);
        public abstract Score GetScore(Item item, State state);
        public abstract Task PostAsync(Item item, State state, CancellationToken token);

        [DebuggerStepThrough]
        async Task<object> IScorable<Item, Score>.PrepareAsync(Item item, CancellationToken token)
        {
            return await this.PrepareAsync(item, token);
        }
        [DebuggerStepThrough]
        bool IScorable<Item, Score>.HasScore(Item item, object opaque)
        {
            var state = (State)opaque;
            return this.HasScore(item, state);
        }
        [DebuggerStepThrough]
        Score IScorable<Item, Score>.GetScore(Item item, object opaque)
        {
            var state = (State)opaque;
            if (!HasScore(item, state))
            {
                throw new InvalidOperationException();
            }

            return this.GetScore(item, state);
        }
        [DebuggerStepThrough]
        Task IScorable<Item, Score>.PostAsync(Item item, object opaque, CancellationToken token)
        {
            try
            {
                var state = (State)opaque;
                return this.PostAsync(item, state, token);
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
    }

    /// <summary>
    /// Provides the state to aggregate the state (and associated scorable) of multiple scorables.
    /// </summary>
    public class Token<Item, Score>
    {
        public IScorable<Item, Score> Scorable;
        public object State;
    }

    /// <summary>
    /// Aggregates some non-empty set of inner scorables to produce an outer scorable.
    /// </summary>
    public abstract class ScorableAggregator<Item, OuterState, OuterScore, InnerState, InnerScore> : ScorableBase<Item, OuterState, OuterScore>
        where OuterState : Token<Item, InnerScore>
    {
        public override bool HasScore(Item item, OuterState state)
        {
            if (state != null)
            {
                return state.Scorable.HasScore(item, state.State);
            }

            return false;
        }
        public override Task PostAsync(Item item, OuterState state, CancellationToken token)
        {
            try
            {
                return state.Scorable.PostAsync(item, state.State, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<Binding>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }
    }
}