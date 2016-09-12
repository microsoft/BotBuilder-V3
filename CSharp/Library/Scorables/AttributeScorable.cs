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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    /// <summary>
    /// Create a dispatch scorable based on attributes.
    /// </summary>
    public sealed class AttributeScorable : DelegatingScorable<IResolver, object>
    {
        public AttributeScorable(Type type, Func<ILuisModel, ILuisService> make)
            : base(Make(type, make))
        {
        }
        public static IEnumerable<A> AttributesFor<A>(MethodInfo method) where A : Attribute
        {
            var m = method.GetCustomAttributes<A>(inherit: true);
            var t = method.DeclaringType.GetCustomAttributes<A>(inherit: true);

            return m.Concat(t);
        }

        public static IScorable<IResolver, object> Make(Type type, Func<ILuisModel, ILuisService> make)
        {
            var methods = type.GetMethods();
            var scorableByMethod = methods.ToDictionary(m => m, m => new MethodScorable(m));

            var luisSpec =
                from method in methods
                from model in AttributesFor<LuisModelAttribute>(method)
                from intent in AttributesFor<LuisIntentAttribute>(method)
                select new { method, intent, model };

            // for a given LUIS model and intent, fold the corresponding method scorables together to enable overload resolution
            var luisScorables =
                from spec in luisSpec
                group spec by new { spec.model, spec.intent } into modelIntents
                let method = modelIntents.Select(m => scorableByMethod[m.method]).ToArray().Fold(Binding.ResolutionComparer.Instance)
                select new LuisIntentScorable<Binding, Binding>(modelIntents.Key.model, modelIntents.Key.intent, method);

            var regexSpec =
                from method in methods
                from pattern in AttributesFor<RegexPatternAttribute>(method)
                select new { method, pattern };

            // for a given regular expression pattern, fold the corresponding method scorables together to enable overload resolution
            var regexScorables =
                from spec in regexSpec
                group spec by spec.pattern into patterns
                let method = patterns.Select(m => scorableByMethod[m.method]).ToArray().Fold(Binding.ResolutionComparer.Instance)
                let regex = new Regex(patterns.Key.Pattern)
                select new RegexMatchScorable<Binding, Binding>(regex, method);

            var methodScorables = scorableByMethod.Values;

            // fold all of the scorables together by score type, and select the best
            IScorable<IResolver, object> regexAll = regexScorables.ToArray().Fold(MatchComparer.Instance);//.Select((_, m) => 1.0);
            IScorable<IResolver, object> luisAll = luisScorables.ToArray().Fold(IntentComparer.Instance);//.Select((_, i) => i.Score.Value);
            IScorable<IResolver, object> methodAll = methodScorables.ToArray().Fold(Binding.ResolutionComparer.Instance);//.Select((_, b) => 1.0);

            // introduce a precedence order to prefer regex over luis over generic method matches
            var all = new[] { regexAll, luisAll, methodAll };

            // select the first scorable that matches
            var winner = all.First();

            // introduce a lifetime scope to the scoring process to memoize service calls to LUIS
            TryResolve tryResolve = (Type t, object tag, out object value) =>
            {
                var model = tag as ILuisModel;
                if (model != null)
                {
                    value = new MemoizingLuisService(make(model));
                    return true;
                }

                value = null;
                return false;
            };
            var scope = new ScopeScorable<object, object>(tryResolve, winner);
            return scope;
        }

        private sealed class MemoizingLuisService : ILuisService
        {
            private readonly ILuisService service;
            private Uri uri;
            private LuisResult result;
            public MemoizingLuisService(ILuisService service)
            {
                SetField.NotNull(out this.service, nameof(service), service);
            }

            Uri ILuisService.BuildUri(string text)
            {
                return this.service.BuildUri(text);
            }

            async Task<LuisResult> ILuisService.QueryAsync(Uri uri, CancellationToken token)
            {
                if (result == null)
                {
                    this.uri = uri;
                    this.result = await this.service.QueryAsync(uri, token);
                }

                if (this.uri != uri)
                {
                    throw new InvalidOperationException();
                }

                return this.result;
            }
        }
    }
}
