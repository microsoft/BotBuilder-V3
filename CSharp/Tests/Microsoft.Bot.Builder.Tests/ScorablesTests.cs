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

using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    // temporary home for special-purpose IScorable
    public sealed class CancelScorable : IScorable<double>
    {
        private readonly IDialogStack stack;
        private readonly Regex regex;
        public CancelScorable(IDialogStack stack, Regex regex)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.regex, nameof(regex), regex);
        }

        async Task<object> IScorable<double>.PrepareAsync<Item>(Item item, Delegate method, CancellationToken token)
        {
            var message = item as Message;
            if (message != null && message.Text != null)
            {
                var text = message.Text;
                var match = regex.Match(text);
                if (match.Success)
                {
                    return match.Length / ((double)text.Length);
                }
            }

            return false;
        }

        bool IScorable<double>.TryScore(object state, out double score)
        {
            if (state is double)
            {
                score = (double)state;
                return true;
            }
            else
            {
                score = double.NaN;
                return false;
            }
        }

        async Task IScorable<double>.PostAsync<Item>(IPostToBot inner, Item item, object state, CancellationToken token)
        {
            this.stack.Fail(new OperationCanceledException());
            await this.stack.PollAsync(token);
        }
    }

    [TestClass]
    public sealed class ScorablesTests : PromptTests_Base
    {
        public const string PromptText = "what is your name?";

        [TestMethod]
        public async Task Scorable_Cancel_Not_Triggered()
        {
            var dialog = MockDialog<string>((context, resume) => PromptDialog.Text(context, resume, PromptText));

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                var builder = new ContainerBuilder();
                builder
                    .Register(c => new CancelScorable(c.Resolve<IDialogStack>(), new Regex("cancel")))
                    .As<IScorable<double>>();
                builder.Update(container);

                var toBot = MakeTestMessage();

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);

                    AssertMentions(PromptText, scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    const string TextNormal = "normal response";

                    var task = scope.Resolve<IPostToBot>();
                    toBot.Text = TextNormal;
                    await task.PostAsync(toBot, CancellationToken.None);

                    dialog
                        .Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<string>>(actual => actual.ToTask().Result == TextNormal)));
                }
            }
        }

        [TestMethod]
        public async Task Scorable_Cancel_Is_Triggered()
        {
            var dialog = MockDialog<string>((context, resume) => PromptDialog.Text(context, resume, PromptText));

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                var builder = new ContainerBuilder();
                builder
                    .Register(c => new CancelScorable(c.Resolve<IDialogStack>(), new Regex("cancel")))
                    .As<IScorable<double>>();
                builder.Update(container);

                var toBot = MakeTestMessage();

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);

                    AssertMentions(PromptText, scope);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, () => dialog.Object);

                    const string TextNormal = "cancel me";

                    var task = scope.Resolve<IPostToBot>();
                    toBot.Text = TextNormal;
                    await task.PostAsync(toBot, CancellationToken.None);

                    dialog
                        .Verify(d => d.PromptResult(It.IsAny<IDialogContext>(), It.Is<IAwaitable<string>>(actual => actual.ToTask().IsFaulted)));
                }
            }
        }
    }
}
