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

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

using Autofac;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class DialogTaskTests : DialogTestBase
    {
        public interface IDialogThatFails : IDialog<object>
        {
            Task MessageReceived(IDialogContext context, IAwaitable<Message> message);
            Task Throw(IDialogContext context, IAwaitable<Message> message);
        }

        [TestMethod]
        public async Task If_Root_Dialog_Throws_Propagate_Exception_Reset_Store()
        {
            var dialog = new Mock<IDialogThatFails>(MockBehavior.Loose);

            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialog.Object.MessageReceived); });

            dialog
                .Setup(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()))
                .Returns<IDialogContext, IAwaitable<Message>>(async (context, result) => { context.Wait(dialog.Object.Throw); });

            dialog
                .Setup(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()))
                .Throws<ApplicationException>();

            Func<IDialog<object>> MakeRoot = () => dialog.Object;
            var toBot = new Message() { ConversationId = Guid.NewGuid().ToString() };

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IDialogTask>();
                    await task.PostAsync(toBot, MakeRoot);
                }

                dialog.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                dialog.Verify(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()), Times.Once);
                dialog.Verify(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()), Times.Never);

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IDialogTask>();
                    Assert.AreNotEqual(0, task.Count);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    try
                    {
                        var task = scope.Resolve<IDialogTask>();
                        await task.PostAsync(toBot, MakeRoot);
                        Assert.Fail();
                    }
                    catch (ApplicationException)
                    {
                    }
                    catch
                    {
                        Assert.Fail();
                    }
                }

                dialog.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Once);
                dialog.Verify(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()), Times.Once);
                dialog.Verify(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()), Times.Once);

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IDialogTask>();
                    Assert.AreEqual(0, task.Count);
                }

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IDialogTask>();
                    await task.PostAsync(toBot, MakeRoot);
                }

                dialog.Verify(d => d.StartAsync(It.IsAny<IDialogContext>()), Times.Exactly(2));
                dialog.Verify(d => d.MessageReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()), Times.Exactly(2));
                dialog.Verify(d => d.Throw(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()), Times.Once);
            }
        }
    }
}
