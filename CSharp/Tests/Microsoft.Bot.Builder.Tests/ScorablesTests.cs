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
using Microsoft.Bot.Builder.Internals.Scorables;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Microsoft.Bot.Builder.Tests
{
    // temporary home for special-purpose IScorable
    public sealed class CancelScorable : ScorableBase<IActivity, double?, double>
    {
        private readonly IDialogStack stack;
        private readonly Regex regex;
        public CancelScorable(IDialogStack stack, Regex regex)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.regex, nameof(regex), regex);
        }

        public override async Task<double?> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;
            if (message != null && message.Text != null)
            {
                var text = message.Text;
                var match = regex.Match(text);
                if (match.Success)
                {
                    return match.Length / ((double)text.Length);
                }
            }

            return null;
        }
        public override bool HasScore(IActivity item, double? state)
        {
            return state.HasValue;
        }
        public override double GetScore(IActivity item, double? state)
        {
            return state.Value;
        }
        public override async Task PostAsync(IActivity item, double? state, CancellationToken token)
        {
            this.stack.Fail(new OperationCanceledException());
            await this.stack.PollAsync(token);
        }
    }

    [TestClass]
    public sealed class CancelScorableTests : PromptTests_Base
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
                    .As<IScorable<IActivity, double>>();
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
                    .As<IScorable<IActivity, double>>();
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

    [Serializable]
    public sealed class CalculatorDialog : IDialog<double>
    {
        async Task IDialog<double>.StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);
        }


        // http://stackoverflow.com/a/2196685
        public static double Evaluate(string expression)
        {
            var regex = new Regex(@"([\+\-\*])");

            var text = regex.Replace(expression, " ${1} ")
                            .Replace("/", " div ")
                            .Replace("%", " mod ");

            var xpath = $"number({text})";
            using (var reader = new StringReader("<r/>"))
            {
                var document = new XPathDocument(reader);
                var navigator = document.CreateNavigator();
                var result = navigator.Evaluate(xpath);
                return (double)result;
            }
        }

        public async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var toBot = await message;
            var value = Evaluate(toBot.Text);
            await context.PostAsync(value.ToString());
            context.Done(value);
        }
    }

    // temporary home for special-purpose IScorable
    public sealed class CalculatorScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogStack stack;
        private readonly Regex regex;
        public CalculatorScorable(IDialogStack stack, Regex regex)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
            SetField.NotNull(out this.regex, nameof(regex), regex);
        }

        public override async Task<string> PrepareAsync(IActivity item, CancellationToken token)
        {
            var message = item as IMessageActivity;
            if (message != null && message.Text != null)
            {
                var text = message.Text;
                var match = regex.Match(text);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }
        public override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }
        public override double GetScore(IActivity item, string state)
        {
            return 1.0;
        }
        public override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var dialog = new CalculatorDialog();

            // let's strip off the prefix in favor of the actual arithmetic expression
            var message = (IMessageActivity)item;
            message.Text = state;

            await this.stack.Forward(dialog.Void<double, IMessageActivity>(), null, message, token);
            await this.stack.PollAsync(token);
        }
    }

    [TestClass]
    public sealed class CalculatorScorableTests : DialogTestBase
    {
        [TestMethod]
        public async Task Scorable_Calculate_Script()
        {
            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            using (var container = Build(Options.ResolveDialogFromContainer))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(echo)
                    .As<IDialog<object>>();
                builder
                    .Register(c => new CalculatorScorable(c.Resolve<IDialogStack>(), new Regex(@".*calculate\s*(.*)")))
                    .As<IScorable<IActivity, double>>();
                builder.Update(container);

                await AssertScriptAsync(container,
                    "hello",
                    "echo: hello",
                    "calculate 2 + 3",
                    "5",
                    "world",
                    "echo: world",
                    "2 + 3",
                    "echo: 2 + 3",
                    "calculate 4 / 2",
                    "2"
                    );
            }
        }
    }
}