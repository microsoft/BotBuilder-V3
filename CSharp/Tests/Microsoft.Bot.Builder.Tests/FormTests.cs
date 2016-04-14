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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Autofac;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Dialogs.Internals;

namespace Microsoft.Bot.Builder.Tests
{
#pragma warning disable CS1998

    [TestClass]
    public sealed class FormTests : DialogTestBase
    {
        public interface IFormTarget
        {
            string Text { get; set; }
            int Integer { get; set; }
            float Float { get; set; }
        }

        [TestMethod]
        public async Task Can_Fill_In_Scalar_Types()
        {
            var mock = new Mock<IFormTarget>();
            mock.SetupAllProperties();

            Func<IDialog<IFormTarget>> MakeRoot = () => new FormDialog<IFormTarget>(mock.Object);

            // arrange
            var toBot = new Message() { ConversationId = Guid.NewGuid().ToString() };

            using (new FiberTests.ResolveMoqAssembly(mock.Object))
            using (var container = Build(mock.Object))
            {
                using (var scope = container.BeginLifetimeScope())
                {
                    var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));

                    // act
                    await store.PostAsync(toBot, MakeRoot);

                    // assert
                    AssertMentions(nameof(mock.Object.Text), scope);
                }

                using (var scope = container.BeginLifetimeScope())
                {
                    var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));

                    // arrange
                    // note: this can not be "text" as that is a navigation command
                    toBot.Text = "words";

                    // act
                    await store.PostAsync(toBot, MakeRoot);

                    // assert
                    AssertMentions(nameof(mock.Object.Integer), scope);
                }


                using (var scope = container.BeginLifetimeScope())
                {
                    var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));
                    // arrange
                    toBot.Text = "3";

                    // act
                    await store.PostAsync(toBot, MakeRoot);

                    // assert
                    AssertMentions(nameof(mock.Object.Float), scope);
                }


                using (var scope = container.BeginLifetimeScope())
                {
                    var store = scope.Resolve<IDialogContextStore>(TypedParameter.From(toBot));

                    // arrange
                    toBot.Text = "3.5";

                    // act
                    await store.PostAsync(toBot, MakeRoot);

                    // assert
                    AssertNoMessages(scope);
                }

                mock.VerifyAll();
            }
        }
    }
}
