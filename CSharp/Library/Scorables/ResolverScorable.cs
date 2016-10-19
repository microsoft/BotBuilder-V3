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

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    public abstract class ResolverScope<InnerScore> : Token<IResolver, InnerScore>, IResolver
    {
        protected readonly IResolver inner;
        public ResolverScope(IResolver inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }
        public virtual bool TryResolve(Type type, object tag, out object value)
        {
            return inner.TryResolve(type, tag, out value);
        }
    }

    public abstract class ResolverScorable<OuterState, OuterScore, InnerState, InnerScore> : ScorableAggregator<IResolver, OuterState, OuterScore, InnerState, InnerScore>
        where OuterState : ResolverScope<InnerScore>
    {
        protected readonly IScorable<IResolver, InnerScore> inner;
        public ResolverScorable(IScorable<IResolver, InnerScore> inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }
    }

    /// <summary>
    /// Scorable for introducing a lifetime scope to resources needed during a IScorable.PrepareAsync.
    /// </summary>
    public sealed class ScopeScorable<InnerState, InnerScore> : ResolverScorable<ScopeScorable<InnerState, InnerScore>.Scope, InnerScore, InnerState, InnerScore>
    {
        public sealed class Scope : ResolverScope<InnerScore>
        {
            private readonly TryResolve tryResolve;
            private readonly Dictionary<object, object> valueByTag = new Dictionary<object, object>();
            public Scope(TryResolve tryResolve, IResolver inner)
                : base(inner)
            {
                SetField.NotNull(out this.tryResolve, nameof(this.tryResolve), tryResolve);
            }
            public override bool TryResolve(Type type, object tag, out object value)
            {
                // TODO: should include type in key?
                if (tag != null)
                {
                    if (valueByTag.TryGetValue(tag, out value))
                    {
                        return true;
                    }

                    if (this.tryResolve(type, tag, out value))
                    {
                        valueByTag.Add(tag, value);
                        return true;
                    }
                }

                return base.TryResolve(type, tag, out value);
            }
        }
        private readonly TryResolve tryResolve;
        public ScopeScorable(TryResolve tryResolve, IScorable<IResolver, InnerScore> inner)
            : base(inner)
        {
            SetField.NotNull(out this.tryResolve, nameof(tryResolve), tryResolve);
        }
        public override async Task<Scope> PrepareAsync(IResolver item, CancellationToken token)
        {
            var state = new Scope(this.tryResolve, item);
            state.Scorable = this.inner;
            state.State = await this.inner.PrepareAsync(state, token);
            return state;
        }
        public override InnerScore GetScore(IResolver item, Scope state)
        {
            return state.Scorable.GetScore(item, state);
        }
    }
}
