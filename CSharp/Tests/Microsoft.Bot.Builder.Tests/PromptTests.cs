using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Bot.Builder.Fibers;

namespace Microsoft.Bot.Builder.Tests
{
#pragma warning disable CS1998

    public abstract class PromptTests_Base
    {
        public static string NewID()
        {
            return Guid.NewGuid().ToString();
        }

        public interface IPromptCaller<T> : IDialog<object>
        {
            Task FirstMessage(IDialogContext context, IAwaitable<Connector.Message> message);
            Task PromptResult(IDialogContext context, IAwaitable<T> result);
        }

        public static Mock<IPromptCaller<T>> MockDialog<T>(string id = null)
        {
            var dialog = new Moq.Mock<IPromptCaller<T>>(MockBehavior.Loose);
            id = id ?? NewID();
            dialog.Setup(d => d.ToString()).Returns(id);
            return dialog;
        }

        public static async Task<DialogContext> MakeContextAsync(IDialog<object> root)
        {
            var data = new Internals.JObjectBotData(new Connector.Message());

            IFiberLoop fiber = new Fiber(new FrameFactory(new WaitFactory()));
            var context = new DialogContext(data, fiber);
            var loop = Methods.Void(Methods.Loop(context.ToRest<object>(root.StartAsync), int.MaxValue));
            fiber.Call(loop, null);
            await fiber.PollAsync();
            return context;
        }
    }


    [TestClass]
    public sealed class PromptTests_Success : PromptTests_Base
    {
        private const string PromptText = "hello there";

        public async Task PromptSuccessAsync<T>(Action<IDialogContext, ResumeAfter<T>> prompt, string text, T expected)
        {
            var dialogRoot = MockDialog<T>();

            dialogRoot
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<object>>()))
                .Returns<IDialogContext, IAwaitable<object>>(async (c, a) => { c.Wait(dialogRoot.Object.FirstMessage); });
            dialogRoot
                .Setup(d => d.FirstMessage(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Connector.Message>>()))
                .Returns<IDialogContext, IAwaitable<object>>(async (c, a) => { prompt(c, dialogRoot.Object.PromptResult); });

            var conversationID = NewID();

            var context = await MakeContextAsync(dialogRoot.Object);
            IUserToBot userToBot = context;
            {
                var toBot = new Connector.Message() { ConversationId = conversationID };
                var toUser = await userToBot.PostAsync(toBot);
                Assert.AreEqual(PromptText, toUser.Text);
            }

            {
                var toBot = new Connector.Message() { ConversationId = conversationID, Text = text };
                var toUser = await userToBot.PostAsync(toBot);
                dialogRoot.Verify(d => d.PromptResult(context, It.Is<IAwaitable<T>>(actual => actual.GetAwaiter().GetResult().Equals(expected))), Times.Once);
            }
        }

        [TestMethod]
        public async Task PromptSuccess_Text()
        {
            await PromptSuccessAsync((context, resume) => Prompts.Text(context, resume, PromptText), "lol wut", "lol wut");
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Yes()
        {
            await PromptSuccessAsync((context, resume) => Prompts.Confirm(context, resume, PromptText), "yes", true);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_No()
        {
            await PromptSuccessAsync((context, resume) => Prompts.Confirm(context, resume, PromptText), "no", false);
        }

        [TestMethod]
        public async Task PromptSuccess_Number()
        {
            await PromptSuccessAsync((context, resume) => Prompts.Number(context, resume, PromptText), "42", 42);
        }

        [TestMethod]
        public async Task PromptSuccess_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((context, resume) => Prompts.Choice(context, resume, choices, PromptText), "two", "two");
        }
    }

    [TestClass]
    public sealed class PromptTests_Failure : PromptTests_Base
    {
        private const string PromptText = "hello there";
        private const string RetryText = "hello there again";
        private const int MaximumAttempts = 2;

        public async Task PromptFailureAsync<T>(Action<IDialogContext, ResumeAfter<T>> prompt)
        {
            var dialogRoot = MockDialog<T>();

            dialogRoot
                .Setup(d => d.StartAsync(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<object>>()))
                .Returns<IDialogContext, IAwaitable<object>>(async (c, a) => { c.Wait(dialogRoot.Object.FirstMessage); });
            dialogRoot
                .Setup(d => d.FirstMessage(It.IsAny<IDialogContext>(), It.IsAny<IAwaitable<Connector.Message>>()))
                .Returns<IDialogContext, IAwaitable<object>>(async (c, a) => { prompt(c, dialogRoot.Object.PromptResult); });

            var conversationID = NewID();

            var context = await MakeContextAsync(dialogRoot.Object);
            IUserToBot userToBot = context;
            {
                var toBot = new Connector.Message() { ConversationId = conversationID };
                var toUser = await userToBot.PostAsync(toBot);
                Assert.AreEqual(PromptText, toUser.Text);
            }
            {
                var toBot = new Connector.Message() { ConversationId = conversationID };
                var toUser = await userToBot.PostAsync(toBot);
                Assert.AreEqual(RetryText, toUser.Text);
            }

            {
                var toBot = new Connector.Message() { ConversationId = conversationID };
                var toUser = await userToBot.PostAsync(toBot);

                dialogRoot.Verify(d => d.PromptResult(context, It.Is<IAwaitable<T>>(actual => actual.ToTask().IsFaulted)), Times.Once);
            }
        }

        [TestMethod]
        public async Task PromptFailure_Number()
        {
            await PromptFailureAsync<int>((context, resume) => Prompts.Number(context, resume, PromptText, RetryText, MaximumAttempts));
        }

        [TestMethod]
        public async Task PromptFailure_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptFailureAsync<string>((context, resume) => Prompts.Choice(context, resume, choices, PromptText, RetryText, MaximumAttempts));
        }

        [TestMethod]
        public async Task PromptFailure_Confirm()
        {
            await PromptFailureAsync<bool>((context, resume) => Prompts.Confirm(context, resume, PromptText, RetryText, MaximumAttempts));
        }
    }
}
