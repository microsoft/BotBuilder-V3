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
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

using Autofac;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class ChainTests : DialogTestBase
    {
        public static void AssertQueryText(string expectedText, ILifetimeScope container)
        {
            var queue = container.Resolve<Queue<IMessageActivity>>();
            var texts = queue.Select(m => m.Text).ToArray();
            // last message is re-prompt, next-to-last is result of query expression
            var actualText = texts.Reverse().ElementAt(1);
            Assert.AreEqual(expectedText, actualText);
        }

        public static IDialog<string> MakeSelectManyQuery()
        {
            var prompts = new[] { "p1", "p2", "p3" };

            var query = from x in new PromptDialog.PromptString(prompts[0], prompts[0], attempts: 1)
                        from y in new PromptDialog.PromptString(prompts[1], prompts[1], attempts: 1)
                        from z in new PromptDialog.PromptString(prompts[2], prompts[2], attempts: 1)
                        select string.Join(" ", x, y, z);

            query = query.PostToUser();

            return query;
        }

        [TestMethod]
        public async Task LinqQuerySyntax_SelectMany()
        {
            var toBot = MakeTestMessage();

            var words = new[] { "hello", "world", "!" };

            using (var container = Build(Options.Reflection))
            {
                foreach (var word in words)
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeSelectManyQuery);

                        var task = scope.Resolve<IPostToBot>();
                        toBot.Text = word;
                        // if we inline the query from MakeQuery into this method, and we use an anonymous method to return that query as MakeRoot
                        // then because in C# all anonymous functions in the same method capture all variables in that method, query will be captured
                        // with the linq anonymous methods, and the serializer gets confused trying to deserialize it all.
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                }

                var expected = string.Join(" ", words);
                AssertQueryText(expected, container);
            }
        }

        public static IDialog<string> MakeSelectQuery()
        {
            const string Prompt = "p1";

            var query = from x in new PromptDialog.PromptString(Prompt, Prompt, attempts: 1)
                        let w = new string(x.Reverse().ToArray())
                        select w;

            query = query.PostToUser();

            return query;
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Select()
        {
            const string Phrase = "hello world";

            using (var container = Build(Options.Reflection))
            {
                var toBot = MakeTestMessage();
                toBot.Text = Phrase;
                
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, MakeSelectQuery);

                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }

                var expected = new string(Phrase.Reverse().ToArray());
                AssertQueryText(expected, container);
            }
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Where_True()
        {
            var query = Chain.PostToChain().Select(m => m.Text).Where(text => text == true.ToString()).PostToUser();

            using (var container = Build(Options.Reflection))
            {
                var toBot = MakeTestMessage();
                toBot.Text = true.ToString();
                
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, () => query);

                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }

                var queue = container.Resolve<Queue<IMessageActivity>>();
                var texts = queue.Select(m => m.Text).ToArray();
                Assert.AreEqual(1, texts.Length);
                Assert.AreEqual(true.ToString(), texts[0]);
            }
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Where_False()
        {
            var query = Chain.PostToChain().Select(m => m.Text).Where(text => text == true.ToString()).PostToUser();

            using (var container = Build(Options.Reflection))
            {
                var toBot = MakeTestMessage();
                toBot.Text = false.ToString();
                
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    DialogModule_MakeRoot.Register(scope, () => query);

                    var task = scope.Resolve<IPostToBot>();
                    try
                    {
                        await task.PostAsync(toBot, CancellationToken.None);
                        Assert.Fail();
                    }
                    catch (Chain.WhereCanceledException)
                    {
                    }
                }

                var queue = container.Resolve<Queue<IMessageActivity>>();
                var texts = queue.Select(m => m.Text).ToArray();
                Assert.AreEqual(0, texts.Length);
            }
        }

        public static IDialog<string> MakeSwitchDialog()
        {
            var toBot = from message in Chain.PostToChain() select message.Text;

            var logic =
                toBot
                .Switch
                (
                    new RegexCase<string>(new Regex("^hello"), (context, text) =>
                    {
                        return "world!";
                    }),
                    new Case<string, string>((txt) => txt == "world", (context, text) =>
                    {
                        return "!";
                    }),
                    new DefaultCase<string, string>((context, text) =>
                   {
                       return text;
                   }
                )
            );

            var toUser = logic.PostToUser();

            return toUser;
        }

        [TestMethod]
        public async Task Switch_Case()
        {
            var toBot = MakeTestMessage();

            var words = new[] { "hello", "world", "echo" };
            var expectedReply = new[] { "world!", "!", "echo" };

            using (var container = Build(Options.Reflection))
            {
                foreach (var word in words)
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeSwitchDialog);

                        var task = scope.Resolve<IPostToBot>();
                        toBot.Text = word;
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                }

                var queue = container.Resolve<Queue<IMessageActivity>>();
                var texts = queue.Select(m => m.Text).ToArray();
                CollectionAssert.AreEqual(expectedReply, texts);
            }
        }

        public static IDialog<string> MakeUnwrapQuery()
        {
            const string Prompt1 = "p1";
            const string Prompt2 = "p2";
            return new PromptDialog.PromptString(Prompt1, Prompt1, attempts: 1).Select(p => new PromptDialog.PromptString(Prompt2, Prompt2, attempts: 1)).Unwrap().PostToUser();
        }

        [TestMethod]
        public async Task Linq_Unwrap()
        {
            var toBot = MakeTestMessage();

            var words = new[] { "hello", "world" };

            using (var container = Build(Options.Reflection))
            {
                foreach (var word in words)
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                    {
                        DialogModule_MakeRoot.Register(scope, MakeUnwrapQuery);

                        var task = scope.Resolve<IPostToBot>();
                        toBot.Text = word;
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                }

                var expected = words.Last();
                AssertQueryText(expected, container);
            }
        }

        [TestMethod]
        public async Task LinqQuerySyntax_Without_Reflection_Surrogate()
        {
            // no environment capture in closures here
            var query = from x in new PromptDialog.PromptString("p1", "p1", 1)
                        from y in new PromptDialog.PromptString("p2", "p2", 1)
                        select string.Join(" ", x, y);

            query = query.PostToUser();

            var words = new[] { "hello", "world" };

            using (var container = Build(Options.None))
            {
                var toBot = MakeTestMessage();

                foreach (var word in words)
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                    {
                        DialogModule_MakeRoot.Register(scope, () => query);

                        var task = scope.Resolve<IPostToBot>();
                        toBot.Text = word;
                        await task.PostAsync(toBot, CancellationToken.None);
                    }
                }

                var expected = string.Join(" ", words);
                AssertQueryText(expected, container);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ClosureCaptureException))]
        public async Task LinqQuerySyntax_Throws_ClosureCaptureException()
        {
            var prompts = new[] { "p1", "p2" };
            var query = new PromptDialog.PromptString(prompts[0], prompts[0], attempts: 1).Select(p => new PromptDialog.PromptString(prompts[1], prompts[1], attempts: 1)).Unwrap().PostToUser();

            using (var container = Build(Options.None))
            {
                var formatter = container.Resolve<IFormatter>();
                using (var stream = new MemoryStream())
                {
                    formatter.Serialize(stream, query);
                }
            }
        }

        [TestMethod]
        public async Task SampleChain_Quiz()
        {
            var quiz = Chain
                .PostToChain()
                .Select(_ => "how many questions?")
                .PostToUser()
                .WaitToBot()
                .Select(m => int.Parse(m.Text))
                .Select(count => Enumerable.Range(0, count).Select(index => Chain.Return($"question {index + 1}?").PostToUser().WaitToBot().Select(m => m.Text)))
                .Fold((l, r) => l + "," + r)
                .Select(answers => "your answers were: " + answers)
                .PostToUser();

            using (var container = Build(Options.ResolveDialogFromContainer))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(quiz)
                    .As<IDialog<object>>();
                builder.Update(container);

                await AssertScriptAsync(container,
                    "hello",
                    "how many questions?",
                    "3",
                    "question 1?",
                    "A",
                    "question 2?",
                    "B",
                    "question 3?",
                    "C",
                    "your answers were: A,B,C"
                    );
            }
        }

        [TestMethod]
        public async Task SampleChain_Joke()
        {
            var joke = Chain
                .PostToChain()
                .Select(m => m.Text)
                .Switch
                (
                    Chain.Case
                    (
                        new Regex("^chicken"),
                        (context, text) =>
                            Chain
                            .Return("why did the chicken cross the road?")
                            .PostToUser()
                            .WaitToBot()
                            .Select(ignoreUser => "to get to the other side")
                    ),
                    Chain.Default<string, IDialog<string>>(
                        (context, text) =>
                            Chain
                            .Return("why don't you like chicken jokes?")
                    )
                )
                .Unwrap()
                .PostToUser().
                Loop();

            using (var container = Build(Options.ResolveDialogFromContainer))
            {
                var builder = new ContainerBuilder();
                builder
                    .RegisterInstance(joke)
                    .As<IDialog<object>>();
                builder.Update(container);

                await AssertScriptAsync(container,
                    "chicken",
                    "why did the chicken cross the road?",
                    "i don't know",
                    "to get to the other side",
                    "anything but chickens",
                    "why don't you like chicken jokes?"
                    );
            }
        }
    }
}