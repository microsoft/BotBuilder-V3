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
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class ChainTests
    {
        public static IContainer Build(bool includeReflection = true)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule());
            if (includeReflection)
            {
                builder.RegisterModule(new ReflectionSurrogateModule());
            }
            builder
                .RegisterType<BotToUserQueue>()
                .Keyed<IBotToUser>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .As<IBotToUser>()
                .SingleInstance();

            return builder.Build();
        }

        public static void AssertQueryText(string expectedText, IContainer container)
        {
            var queue = container.Resolve<BotToUserQueue>();
            var texts = queue.Messages.Select(m => m.Text).ToArray();
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
            var toBot = new Message()
            {
                ConversationId = Guid.NewGuid().ToString()
            };

            var words = new [] { "hello", "world", "!" };

            using (var container = Build())
            {
                foreach (var word in words)
                {
                    using (var scope = container.BeginLifetimeScope())
                    {
                        var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));
                        toBot.Text = word;
                        // if we inline the query from MakeQuery into this method, and we use an anonymous method to return that query as MakeRoot
                        // then because in C# all anonymous functions in the same method capture all variables in that method, query will be captured
                        // with the linq anonymous methods, and the serializer gets confused trying to deserialize it all.
                        await store.PostAsync(toBot, MakeSelectManyQuery);
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

            using (var container = Build())
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var toBot = new Message()
                    {
                        ConversationId = Guid.NewGuid().ToString(),
                        Text = Phrase
                    };

                    var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));
                    await store.PostAsync(toBot, MakeSelectQuery);
                }

                var expected = new string(Phrase.Reverse().ToArray());
                AssertQueryText(expected, container);
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
            var toBot = new Message()
            {
                ConversationId = Guid.NewGuid().ToString()
            };

            var words = new[] { "hello", "world" };

            using (var container = Build())
            {
                foreach (var word in words)
                {
                    using (var scope = container.BeginLifetimeScope())
                    {
                        var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));
                        toBot.Text = word;
                        await store.PostAsync(toBot, MakeUnwrapQuery);
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

            using (var container = Build(includeReflection: false))
            {
                var toBot = new Message()
                {
                    ConversationId = Guid.NewGuid().ToString()
                };

                foreach (var word in words)
                {
                    using (var scope = container.BeginLifetimeScope())
                    {
                        var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));
                        toBot.Text = word;
                        await store.PostAsync(toBot, () => query);
                    }
                }

                var expected = string.Join(" ", words);
                AssertQueryText(expected, container);
            }
        }
    }
}
