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
        public static IDialog<string> MakeQuery()
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
        public async Task SelectMany()
        {
            var toBot = new Message()
            {
                ConversationId = Guid.NewGuid().ToString()
            };

            var words = new [] { "hello", "world", "!" };

            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule());
            builder.RegisterModule(new ReflectionSurrogateModule());
            builder
                .RegisterType<BotToUserQueue>()
                .Keyed<IBotToUser>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .As<IBotToUser>()
                .SingleInstance();
            using (var container = builder.Build())
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
                        await store.PostAsync(toBot, MakeQuery);
                    }
                }

                var queue = container.Resolve<BotToUserQueue>();
                // last message is re-prompt, next-to-last is result of query expression
                var toUser = queue.Messages.Reverse().ElementAt(1);
                var expected = string.Join(" ", words);
                Assert.AreEqual(expected, toUser.Text);
            }
        }
    }
}
