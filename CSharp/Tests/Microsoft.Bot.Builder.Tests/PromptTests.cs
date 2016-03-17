using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class PromptTests_Success : DialogFlowTests
    {
        private const string PromptText = "hello there";

        public async Task PromptSuccessAsync(Func<ISession, Task<object>, Task<Connector.Message>> prompt, string text, object expected)
        {
            var dialogRoot = MockDialog();

            dialogRoot
                .Setup(d => d.BeginAsync(It.IsAny<ISession>(), Tasks.Null))
                .Returns<ISession, Task<object>>(prompt);

            ISessionData data = new InMemorySessionData();

            var conversationID = NewID();

            {
                var message = new Connector.Message() { ConversationId = conversationID };
                var session = MakeSession(data, message, dialogRoot);

                var responsePrompt = await session.DispatchAsync();
                Assert.AreEqual(PromptText, responsePrompt.Text);

                ((IDialogStack)session.Stack).Flush();
            }

            {
                var message = new Connector.Message() { ConversationId = conversationID, Text = text };
                var session = MakeSession(data, message, dialogRoot);

                var responsePrompt = await session.DispatchAsync();

                dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is<Task<object>>(actual => actual.Result.Equals(expected))), Times.Once);
            }
        }

        [TestMethod]
        public async Task PromptSuccess_Text()
        {
            await PromptSuccessAsync((session, task) => Prompts.Text(session, PromptText), "lol wut", "lol wut");
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_Yes()
        {
            await PromptSuccessAsync((session, task) => Prompts.Confirm(session, PromptText), "yes", true);
        }

        [TestMethod]
        public async Task PromptSuccess_Confirm_No()
        {
            await PromptSuccessAsync((session, task) => Prompts.Confirm(session, PromptText), "no", false);
        }

        [TestMethod]
        public async Task PromptSuccess_Number()
        {
            await PromptSuccessAsync((session, task) => Prompts.Number(session, PromptText), "42", 42.0);
        }

        [TestMethod]
        public async Task PromptSuccess_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptSuccessAsync((session, task) => Prompts.Choice(session, PromptText, choices), "two", "two");
        }
    }

    [TestClass]
    public sealed class PromptTests_Failure : DialogFlowTests
    {
        private const string PromptText = "hello there";
        private const string RetryPromptText = "hello there again";
        private const int MaximumRetries = 1;

        public async Task PromptFailureAsync(Func<ISession, Task<object>, Task<Connector.Message>> prompt)
        {
            var dialogRoot = MockDialog();

            dialogRoot
                .Setup(d => d.BeginAsync(It.IsAny<ISession>(), Tasks.Null))
                .Returns<ISession, Task<object>>(prompt);

            var session = MakeSession(dialogRoot);

            var responsePrompt = await session.DispatchAsync();
            Assert.AreEqual(PromptText, responsePrompt.Text);

            var responseRetryPrompt = await session.DispatchAsync();
            Assert.AreEqual(RetryPromptText, responseRetryPrompt.Text);

            var responseCancel = await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is<Task<object>>(actual => actual.IsCanceled)), Times.Once);
        }

        [TestMethod]
        public async Task PromptFailure_Number()
        {
            await PromptFailureAsync((session, task) => Prompts.Number(session, PromptText, RetryPromptText, MaximumRetries));
        }

        [TestMethod]
        public async Task PromptFailure_Choice()
        {
            var choices = new[] { "one", "two", "three" };
            await PromptFailureAsync((session, task) => Prompts.Choice(session, PromptText, choices, RetryPromptText, MaximumRetries));
        }

        [TestMethod]
        public async Task PromptFailure_Confirm()
        {
            await PromptFailureAsync((session, task) => Prompts.Confirm(session, PromptText, RetryPromptText, MaximumRetries));
        }
    }
}
