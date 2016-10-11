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
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Internals.Scorables;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A dialog specialized to dispatch an IScorable.
    /// </summary>
    [Serializable]
    public class DispatchDialog<R> : IDialog<R>
    {
        public virtual async Task StartAsync(IDialogContext context)
        {
            context.Wait(ActivityReceivedAsync);
        }

        protected virtual IResolver MakeResolver(IDialogContext context, IActivity activity)
        {
            var resolver = NullResolver.Instance;

            var serviceByType = new Dictionary<Type, object>();
            DictionaryResolver.AddBases(serviceByType, typeof(IDialogContext), context);
            DictionaryResolver.AddBases(serviceByType, typeof(IActivity), activity);
            DictionaryResolver.AddBases(serviceByType, this.GetType(), this);
            resolver = new DictionaryResolver(serviceByType, resolver);

            resolver = new ActivityResolver(resolver);

            return resolver;
        }
        protected virtual ILuisService MakeService(ILuisModel model)
        {
            return new LuisService(model);
        }
        protected virtual IEnumerable<MethodInfo> MakeMethods(IDialogContext context, IActivity activity)
        {
            return this.GetType().GetMethods();
        }
        protected virtual IScorableFactory<IResolver, object> MakeFactory(IDialogContext context, IActivity activity)
        {
            var cache = new DictionaryCache<ILuisService, Uri, LuisResult>(EqualityComparer<Uri>.Default);

            var serviceByModel = new Dictionary<ILuisModel, ILuisService>();

            Func<ILuisModel, ILuisService> MakeLuisService = model =>
            {
                ILuisService service;
                if (!serviceByModel.TryGetValue(model, out service))
                {
                    service = new CachingLuisService(MakeService(model), cache);
                    serviceByModel.Add(model, service);
                }

                return service;
            };

            IScorableFactory<IResolver, object> factory = new OrderScorableFactory<IResolver, object>
                (
                    new LuisIntentScorableFactory(MakeLuisService),
                    new RegexMatchScorableFactory(),
                    new MethodScorableFactory()
                );

            return factory;
        }
        protected virtual IScorable<IResolver, object> MakeScorable(IDialogContext context, IActivity activity)
        {
            var factory = MakeFactory(context, activity);
            var methods = MakeMethods(context, activity);
            var scorable = factory.ScorableFor(methods);
            return scorable;
        }

        protected virtual async Task OnPostAsync(IDialogContext context, IActivity activity)
        {
            context.Wait(ActivityReceivedAsync);
        }

        protected virtual async Task OnFailAsync(IDialogContext context, IActivity activity)
        {
            context.Wait(ActivityReceivedAsync);
        }

        protected virtual async Task ActivityReceivedAsync(IDialogContext context, IAwaitable<IActivity> item)
        {
            var activity = await item;
            var scorable = MakeScorable(context, activity);
            var resolver = MakeResolver(context, activity);

            if (await scorable.TryPostAsync(resolver, context.CancellationToken))
            {
                await OnPostAsync(context, activity);
            }
            else
            {
                await OnFailAsync(context, activity);
            }
        }
    }
}