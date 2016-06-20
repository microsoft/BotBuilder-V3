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
    public abstract class DialogTestBase
    {
        protected static MockConnectorFactory mockConnectorFactory = new MockConnectorFactory(new BotIdResolver("testBot")); 

        [Flags]
        public enum Options { None, Reflection, ScopedQueue, MockConnectorFactory, ResolveDialogFromContainer };

        public static IContainer Build(Options options, params object[] singletons)
        {
            var builder = new ContainerBuilder();
            if (options.HasFlag(Options.ResolveDialogFromContainer))
            {
                builder.RegisterModule(new DialogModule());
            }
            else
            {
                builder.RegisterModule(new DialogModule_MakeRoot());
            }

            builder
                .Register(c => new BotIdResolver("testBot"))
                .As<IBotIdResolver>()
                .SingleInstance();
            
            builder
           .Register((c, p) => mockConnectorFactory)
               .As<IConnectorClientFactory>()
               .InstancePerLifetimeScope();

            if (options.HasFlag(Options.Reflection))
            {
                builder.RegisterModule(new ReflectionSurrogateModule());
            }

            var r =
                builder
                .Register<Queue<IMessageActivity>>(c => new Queue<IMessageActivity>())
                .AsSelf();

            if (options.HasFlag(Options.ScopedQueue))
            {
                r.InstancePerLifetimeScope();
            }
            else
            {
                r.SingleInstance();
            }

            builder
                .RegisterType<BotToUserQueue>()
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            foreach (var singleton in singletons)
            {
                builder
                    .Register(c => singleton)
                    .Keyed(FiberModule.Key_DoNotSerialize, singleton.GetType());
            }

            return builder.Build();
        }

        public static class ChannelID
        {
            public const string User = "testUser";
            public const string Bot = "testBot";
        }

        public static IMessageActivity MakeTestMessage()
        {
            return new Activity() {
                From = new ChannelAccount { Id = ChannelID.User },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                Recipient = new ChannelAccount { Id = ChannelID.Bot },
                ServiceUrl = "InvalidServiceUrl", 
                ChannelId = "Test"
            };
        }

        public static async Task AssertScriptAsync(ILifetimeScope container, params string[] pairs)
        {
            Assert.AreNotEqual(0, pairs.Length);

            var toBot = MakeTestMessage();

            for (int index = 0; index < pairs.Length; ++index)
            {
                var toBotText = pairs[index];

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IPostToBot>();
                    toBot.Text = toBotText;
                    await task.PostAsync(toBot, CancellationToken.None);
                }

                var queue = container.Resolve<Queue<IMessageActivity>>();

                while (queue.Count > 0)
                {
                    ++index;

                    var toUser = queue.Dequeue();

                    var actual = toUser.Text;
                    var expected = pairs[index];

                    Assert.AreEqual(expected, actual);
                }
            }
        }

        public static void AssertMentions(string expectedText, IEnumerable<IMessageActivity> actualToUser)
        {
            Assert.AreEqual(1, actualToUser.Count());
            var index = actualToUser.Single().Text.IndexOf(expectedText, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(index >= 0);
        }

        public static void AssertMentions(string expectedText, ILifetimeScope scope)
        {
            var queue = scope.Resolve<Queue<IMessageActivity>>();
            AssertMentions(expectedText, queue);
        }

        public static void AssertNoMessages(ILifetimeScope scope)
        {
            var queue = scope.Resolve<Queue<IMessageActivity>>();
            Assert.AreEqual(0, queue.Count);
        }

        public static string NewID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
