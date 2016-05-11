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
using Microsoft.Bot.Builder.Internals.Fibers;
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
                    Assert.AreNotEqual(0, task.Frames.Count);
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
                    Assert.AreEqual(0, task.Frames.Count);
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

        public interface IDialogFrames<T> : IDialog<T>
        {
            Task ItemReceived<R>(IDialogContext context, IAwaitable<R> item);
        }

        [TestMethod]
        public async Task DialogTask_Frames()
        {
            var dialog = new Mock<IDialogFrames<object>>(MockBehavior.Loose);

            dialog
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { PromptDialog.Text(context, dialog.Object.ItemReceived, "blah"); });

            Func<IDialog<object>> MakeRoot = () => dialog.Object;
            var toBot = new Message() { ConversationId = Guid.NewGuid().ToString() };

            using (new FiberTestBase.ResolveMoqAssembly(dialog.Object))
            using (var container = Build(Options.None, dialog.Object))
            {
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IDialogTask>();

                    Assert.AreEqual(0, task.Frames.Count);

                    await task.PostAsync(toBot, MakeRoot);

                    Assert.AreEqual(3, task.Frames.Count);
                    Assert.IsInstanceOfType(task.Frames[0].Target, typeof(PromptDialog.PromptString));
                    Assert.IsInstanceOfType(task.Frames[1].Target, dialog.Object.GetType());
                }
            }
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring()
        {
            var dialogOne = new Mock<IDialogFrames<int>>(MockBehavior.Loose);
            var dialogTwo = new Mock<IDialogFrames<Guid>>(MockBehavior.Loose);
            var dialogNew = new Mock<IDialogFrames<DateTime>>(MockBehavior.Loose);

            const string TriggerTextTwo = "foo";
            const string TriggerTextNew = "bar";

            // IDialogFrames<T> StartAsync and ItemReceived

            dialogOne
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Call(dialogTwo.Object, dialogOne.Object.ItemReceived); });

            dialogOne
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Guid>>()))
                .Returns<IDialogContext, IAwaitable<Guid>>(async (context, message) => { context.Wait(dialogOne.Object.ItemReceived); });

            dialogTwo
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialogTwo.Object.ItemReceived); });

            dialogTwo
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()))
                .Returns<IDialogContext, IAwaitable<Message>>(async (context, message) =>
                {
                    if ((await message).Text == TriggerTextTwo)
                    {
                        context.Done(Guid.NewGuid());
                    }
                    else
                    {
                        context.Wait(dialogTwo.Object.ItemReceived);
                    }
                });

            dialogNew
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>()))
                .Returns<IDialogContext>(async context => { context.Wait(dialogNew.Object.ItemReceived); });

            dialogNew
                .Setup(d => d.ItemReceived(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Message>>()))
                .Returns<IDialogContext, IAwaitable<Message>>(async (context, message) => { context.Done(DateTime.UtcNow); });

            // ScoringDialogTask.IScorable

            dialogOne
                .As<IScorable<double>>()
                .Setup(s => s.PrepareAsync(It.IsAny<Message>(), It.IsAny<Delegate>()))
                .Returns<Message, Delegate>(async (m, d) => m);

            double scoreOne = 1.0;
            dialogOne
                .As<IScorable<double>>()
                .Setup(s => s.TryScore(It.IsAny<Message>(), out scoreOne))
                .Returns<Message, double>((m, s) => m.Text == TriggerTextNew);

            dialogOne
                .As<IScorable<double>>()
                .Setup(s => s.PostAsync(It.IsAny<IDialogTask>(), It.IsAny<Message>(), It.IsAny<Message>()))
                .Returns<IDialogTask, Message, Message>(async (task, message, state) =>
                {
                    task.Call(dialogNew.Object.Void<DateTime, Message>(), null);
                });

            dialogTwo
                .As<IScorable<double>>()
                .Setup(s => s.PrepareAsync(It.IsAny<Message>(), It.IsAny<Delegate>()))
                .Returns<Message, Delegate>(async (m, d) => m);

            double scoreTwo = 0.5;
            dialogTwo
                .As<IScorable<double>>()
                .Setup(s => s.TryScore(It.IsAny<Message>(), out scoreTwo))
                .Returns<Message, double>((m, s) => m.Text == TriggerTextNew);

            Func<IDialog<int>> MakeRoot = () => dialogOne.Object;
            var toBot = new Message() { ConversationId = Guid.NewGuid().ToString() };

            using (new FiberTestBase.ResolveMoqAssembly(dialogOne.Object, dialogTwo.Object, dialogNew.Object))
            using (var container = Build(Options.None, dialogOne.Object, dialogTwo.Object, dialogNew.Object))
            {
                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IDialogTask>();

                    // the stack is empty when we first start
                    Assert.AreEqual(0, task.Frames.Count);

                    await task.LoadAsync(MakeRoot);

                    // now the stack has the looping root frame plus the 1st and 2nd active dialogs
                    Assert.AreEqual(3, task.Frames.Count);
                    Assert.AreEqual(dialogTwo.Object, task.Frames[0].Target);
                    Assert.AreEqual(dialogOne.Object, task.Frames[1].Target);

                    await task.PostAsync(toBot, MakeRoot);

                    // nothing special in the message, so we still have the 1st and 2nd active dialogs
                    Assert.AreEqual(3, task.Frames.Count);
                    Assert.AreEqual(dialogTwo.Object, task.Frames[0].Target);
                    Assert.AreEqual(dialogOne.Object, task.Frames[1].Target);

                    toBot.Text = TriggerTextNew;

                    await task.PostAsync(toBot, MakeRoot);

                    // now the trigger has occurred - the interrupting dialog is at the top of the stack,
                    // then the void dialog, then the existing 1st and 2nd dialogs that were interrupted
                    Assert.AreEqual(5, task.Frames.Count);
                    Assert.AreEqual(dialogNew.Object, task.Frames[0].Target);
                    Assert.AreEqual(dialogTwo.Object, task.Frames[2].Target);
                    Assert.AreEqual(dialogOne.Object, task.Frames[3].Target);

                    toBot.Text = string.Empty;

                    await task.PostAsync(toBot, MakeRoot);

                    // now the interrupted dialog will exit, and the void dialog is waiting for original message that
                    // the 2nd dialog had wanted
                    Assert.AreEqual(4, task.Frames.Count);
                    Assert.AreEqual(dialogTwo.Object, task.Frames[1].Target);
                    Assert.AreEqual(dialogOne.Object, task.Frames[2].Target);

                    toBot.Text = TriggerTextTwo;

                    await task.PostAsync(toBot, MakeRoot);

                    // and now that the void dialog was able to capture the message, it returns it to the 2nd dialog,
                    // which returns a guid to the 1st dialog
                    Assert.AreEqual(2, task.Frames.Count);
                    Assert.AreEqual(dialogOne.Object, task.Frames[0].Target);
                }
            }

            dialogOne.VerifyAll();
            dialogTwo.VerifyAll();
            dialogNew.VerifyAll();
        }

        public static Mock<IScorable<T>> MockScorable<T>(object item, Delegate method, object state, T score)
        {
            var scorable = new Mock<IScorable<T>>(MockBehavior.Strict);

            scorable
                .Setup(s => s.PrepareAsync(item, method))
                .ReturnsAsync(state);

            scorable
                .Setup(s => s.TryScore(state, out score))
                .Returns(true);

            return scorable;
        }

        public static async Task DialogTask_Frame_Scoring_Allows_Value(double score)
        {
            var state = new object();
            var item = new object();
            var scorable = MockScorable(item, null, state, score);

            var inner = new Mock<IDialogTask>();
            var task = new ScoringDialogTask<double>(inner.Object, Comparer<double>.Default, new NormalizedTraits(), scorable.Object);
            inner
                .SetupGet(i => i.Frames)
                    .Returns(Array.Empty<Delegate>());

            scorable
                .Setup(s => s.PostAsync(inner.Object, item, state))
                .Returns(Task.FromResult(0));

            await task.PostAsync(item);

            scorable.Verify();
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Allows_Minimum()
        {
            await DialogTask_Frame_Scoring_Allows_Value(0.0);
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Allows_Maximum()
        {
            await DialogTask_Frame_Scoring_Allows_Value(1.0);
        }

        public static async Task DialogTask_Frame_Scoring_Throws_Out_Of_Range(double score)
        {
            var state = new object();
            var item = new object();
            var scorable = MockScorable(item, null, state, score);

            var inner = new Mock<IDialogTask>();
            var task = new ScoringDialogTask<double>(inner.Object, Comparer<double>.Default, new NormalizedTraits(), scorable.Object);
            inner
                .SetupGet(i => i.Frames)
                    .Returns(Array.Empty<Delegate>());

            try
            {
                await task.PostAsync(item);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            scorable.Verify();
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Throws_Too_Large()
        {
            await DialogTask_Frame_Scoring_Throws_Out_Of_Range(1.1);
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Throws_Too_Small()
        {
            await DialogTask_Frame_Scoring_Throws_Out_Of_Range(-0.1);
        }

        [TestMethod]
        public async Task DialogTask_Frame_Scoring_Stops_At_Maximum()
        {
            var state1 = new object();
            var item = new object();
            var scorable1 = MockScorable(item, null, state1, 1.0);
            var scorable2 = new Mock<IScorable<double>>(MockBehavior.Strict);

            var inner = new Mock<IDialogTask>();
            var task = new ScoringDialogTask<double>(inner.Object, Comparer<double>.Default, new NormalizedTraits(), scorable1.Object, scorable2.Object);
            inner
                .SetupGet(i => i.Frames)
                .Returns(Array.Empty<Delegate>());

            scorable1
                .Setup(s => s.PostAsync(inner.Object, item, state1))
                .Returns(Task.FromResult(0));

            await task.PostAsync(item);

            scorable1.Verify();
            scorable2.Verify();
        }
    }
}
