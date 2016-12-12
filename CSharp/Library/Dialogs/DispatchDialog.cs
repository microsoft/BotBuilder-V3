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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Scorables;

namespace Microsoft.Bot.Builder.Dialogs
{
    /// <summary>
    /// A dialog specialized to dispatch an IScorable.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    [Serializable]
    public class DispatchDialog<TResult> : IDialog<TResult>
    {
        public virtual async Task StartAsync(IDialogContext context)
        {
            context.Wait(ActivityReceivedAsync);
        }

        protected virtual IResolver MakeResolver(IDialogContext context, IActivity activity)
        {
            var resolver = NoneResolver.Instance;
            resolver = new ArrayResolver(resolver, context, activity, this);
            resolver = new ActivityResolver(resolver);

            return resolver;
        }

        protected virtual ILuisService MakeService(ILuisModel model)
        {
            return new LuisService(model);
        }

        protected virtual Regex MakeRegex(string pattern)
        {
            return new Regex(pattern);
        }

        protected virtual BindingFlags MakeBindingFlags()
        {
            return BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        }

        protected virtual IEnumerable<MethodInfo> MakeMethods(IDialogContext context, IActivity activity)
        {
            var type = this.GetType();
            var flags = this.MakeBindingFlags();
            var methods = type.GetMethods(flags);
            return methods;
        }

        private bool continueAfterPost;

        protected void ContinueWithNextGroup()
        {
            continueAfterPost = true;
        }

        protected virtual bool OnStage(FoldStage stage, IScorable<IResolver, object> scorable, IResolver item, object state, object score)
        {
            switch (stage)
            {
                case FoldStage.AfterFold: return true;
                case FoldStage.StartPost: continueAfterPost = false; return true;
                case FoldStage.AfterPost: return continueAfterPost;
                default: throw new NotImplementedException();
            }
        }

        protected virtual IComparer<object> MakeComparer(IDialogContext context, IActivity activity)
        {
            return NullComparer<object>.Instance;
        }

        protected virtual IScorableFactory<IResolver, object> MakeFactory(IDialogContext context, IActivity activity)
        {
            var comparer = MakeComparer(context, activity);

            IScorableFactory<IResolver, object> factory = new OrderScorableFactory<IResolver, object>
                (
                    this.OnStage,
                    comparer,
                    new LuisIntentScorableFactory(MakeService),
                    new RegexMatchScorableFactory(MakeRegex),
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
        }

        protected virtual async Task OnFailAsync(IDialogContext context, IActivity activity)
        {
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

    /// <summary>
    /// A dialog specialized to dispatch an IScorable.
    /// </summary>
    /// <remarks>
    /// This non-generic dialog is intended for use as a top-level dialog that will not
    /// return to any calling parent dialog (and therefore the result type is object).
    /// </remarks>
    [Serializable]
    public class DispatchDialog : DispatchDialog<object>
    {
    }
}