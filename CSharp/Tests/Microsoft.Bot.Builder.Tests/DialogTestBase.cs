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

using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Tests
{
    public abstract class DialogTestBase
    {
        public static IContainer Build(params object[] singletons)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule());

            builder
                .RegisterType<BotToUserQueue>()
                .Keyed<IBotToUser>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            foreach (var singleton in singletons)
            {
                builder
                    .Register(c => singleton)
                    .Keyed<object>(FiberModule.Key_DoNotSerialize);
            }

            return builder.Build();
        }

        public static void AssertMentions(string expectedText, IEnumerable<Message> actualToUser)
        {
            Assert.AreEqual(1, actualToUser.Count());
            var index = actualToUser.Single().Text.IndexOf(expectedText, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(index >= 0);
        }

        public static void AssertMentions(string expectedText, ILifetimeScope scope)
        {
            var queue = scope.Resolve<BotToUserQueue>();
            AssertMentions(expectedText, queue.Messages);
        }

        public static void AssertNoMessages(ILifetimeScope scope)
        {
            var queue = scope.Resolve<BotToUserQueue>();
            Assert.AreEqual(0, queue.Messages.Count());
        }

        public static string NewID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
