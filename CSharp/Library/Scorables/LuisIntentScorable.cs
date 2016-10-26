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
    /// <summary>
    /// Scorable to represent a specific LUIS intent recommendation.
    /// </summary>
    public sealed class LuisIntentScorable<InnerState, InnerScore> : ResolverScorable<LuisIntentScorable<InnerState, InnerScore>.Scope, IntentRecommendation, InnerState, InnerScore>
    {
        private readonly ILuisModel model;
        private readonly LuisIntentAttribute intent;

        public sealed class Scope : ResolverScope<InnerScore>
        {
            public readonly ILuisModel Model;
            public readonly LuisResult Result;
            public readonly IntentRecommendation Intent;
            public Scope(ILuisModel model, LuisResult result, IntentRecommendation intent, IResolver inner)
                : base(inner)
            {
                SetField.NotNull(out this.Model, nameof(model), model);
                SetField.NotNull(out this.Result, nameof(result), result);
                SetField.NotNull(out this.Intent, nameof(intent), intent);
            }
            public override bool TryResolve(Type type, object tag, out object value)
            {
                if (type.IsAssignableFrom(typeof(ILuisModel)))
                {
                    value = this.Model;
                    return true;
                }
                if (type.IsAssignableFrom(typeof(LuisResult)))
                {
                    value = this.Result;
                    return true;
                }
                if (type.IsAssignableFrom(typeof(IntentRecommendation)))
                {
                    value = this.Intent;
                    return true;
                }

                var name = tag as string;
                if (name != null)
                {
                    var typeE = type.IsAssignableFrom(typeof(EntityRecommendation));
                    var typeS = type.IsAssignableFrom(typeof(string));
                    var typeIE = type.IsAssignableFrom(typeof(IReadOnlyList<EntityRecommendation>));
                    var typeIS = type.IsAssignableFrom(typeof(IReadOnlyList<string>));
                    if (typeE || typeS || typeIE || typeIS)
                    {
                        var entities = this.Result.Entities.Where(e => e.Type == name).ToArray();
                        if (entities.Length > 0)
                        {
                            if (entities.Length == 1)
                            {
                                if (typeE)
                                {
                                    value = entities[0];
                                    return true;
                                }
                                if (typeS)
                                {
                                    value = entities[0].Entity;
                                    return true;
                                }
                            }

                            if (typeIE)
                            {
                                value = entities;
                                return true;
                            }
                            if (typeIS)
                            {
                                value = entities.Select(e => e.Entity).ToArray();
                                return true;
                            }
                        }
                        // TODO: parsing and interpretation of LUIS entity resolutions
                    }
                }

                // i.e. for IActivity
                return base.TryResolve(type, tag, out value);
            }
        }

        public LuisIntentScorable(ILuisModel model, LuisIntentAttribute intent, IScorable<IResolver, InnerScore> inner)
            : base(inner)
        {
            SetField.NotNull(out this.model, nameof(model), model);
            SetField.NotNull(out this.intent, nameof(intent), intent);
        }
        public override async Task<Scope> PrepareAsync(IResolver resolver, CancellationToken token)
        {
            IMessageActivity message;
            if (!resolver.TryResolve(null, out message))
            {
                return null;
            }

            var text = message.Text;
            if (text == null)
            {
                return null;
            }

            ILuisService service;
            if (! resolver.TryResolve(this.model, out service))
            {
                return null;
            }

            var result = await service.QueryAsync(text, token);
            var intents = result.Intents;
            if (intents == null)
            {
                return null;
            }

            var intent = intents.SingleOrDefault(i => i.Intent.Equals(this.intent.IntentName, StringComparison.OrdinalIgnoreCase));
            if (intent == null)
            {
                return null;
            }

            if (!intent.Score.HasValue)
            {
                return null;
            }

            var scope = new Scope(this.model, result, intent, resolver);
            scope.Scorable = this.inner;
            scope.State = await this.inner.PrepareAsync(scope, token);
            return scope;
        }
        public override IntentRecommendation GetScore(IResolver resolver, Scope state)
        {
            return state.Intent;
        }
    }

    public sealed class IntentComparer : IComparer<IntentRecommendation>
    {
        public static readonly IComparer<IntentRecommendation> Instance = new IntentComparer();
        private IntentComparer()
        {
        }
        int IComparer<IntentRecommendation>.Compare(IntentRecommendation one, IntentRecommendation two)
        {
            Func<IntentRecommendation, Pair<bool, double>> PairFor = intent => Pair.Create
            (
                ! intent.Intent.Equals("none", StringComparison.OrdinalIgnoreCase),
                intent.Score.GetValueOrDefault()
            );

            var pairOne = PairFor(one);
            var pairTwo = PairFor(two);
            return pairOne.CompareTo(pairTwo);
        }
    }
}
