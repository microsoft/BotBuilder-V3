using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.Bot.Builder.Tests
{
    public abstract class DialogFlowTests
    {
        public static string NewID()
        {
            return Guid.NewGuid().ToString();
        }

        public static Mock<IDialog> MockDialog(string id = null)
        {
            var dialog = new Moq.Mock<IDialog>(MockBehavior.Loose);
            dialog.SetupGet(d => d.ID).Returns(id ?? NewID());
            dialog.Setup(d => d.ToString()).Returns(id);
            return dialog;
        }

        public static ISession MakeSession(ISessionData data, Connector.Message message, params Moq.Mock<IDialog>[] mocks)
        {
            var objects = mocks.Select(mock => mock.Object);
            IDialogCollection dialogs = new DialogCollection(objects);
            IDialogStack stack = new DialogStack(data, dialogs);
            ISession session = new ConsoleSession(message, data, stack, objects.First());
            return session;
        }

        public static ISession MakeSession(params Moq.Mock<IDialog>[] mocks)
        {
            ISessionStore store = new InMemoryStore();
            ISessionData data = new InMemorySessionData();
            var message = new Connector.Message() { ConversationId = NewID(), Text = string.Empty };
            return MakeSession(data, message, mocks);
        }

        protected sealed class MockException : Exception
        {
            public static readonly Task<object> Task = Task<object>.Factory.FromException(new MockException());
        }

        public static Expression<Func<Task<object>, bool>> ExceptionOfType<E>() where E : Exception
        {
            return task => task.IsFaulted && task.Exception.InnerException is E;
        }

        public static async Task AssertExceptionAsync<T>(Task<T> task, Type type = null)
        {
            type = type ?? typeof(MockException);

            await task.WithNoThrow();
            Assert.IsTrue(task.IsFaulted);
            Assert.IsInstanceOfType(task.Exception.InnerException, type);
        }
    }

    [TestClass]
    public sealed class DialogFlowTests_CallOrder : DialogFlowTests
    {
        [TestMethod]
        public async Task Root_Begin_Before_Reply()
        {
            var dialogRoot = MockDialog();

            var message1 = new Connector.Message();
            var message2 = new Connector.Message();

            var session = MakeSession(dialogRoot);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).ReturnsAsync(message1);
            dialogRoot.Setup(d => d.ReplyReceivedAsync(session)).ReturnsAsync(message2);

            var response1 = await session.DispatchAsync();
            Assert.AreEqual(message1, response1);

            var response2 = await session.DispatchAsync();
            Assert.AreEqual(message2, response2);
        }

        [TestMethod]
        public async Task Leaf_Begin_Before_Reply()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var message1 = new Connector.Message();
            var message2 = new Connector.Message();

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).ReturnsAsync(message1);
            dialogLeaf.Setup(d => d.ReplyReceivedAsync(session)).ReturnsAsync(message2);

            var response1 = await session.DispatchAsync();
            Assert.AreEqual(message1, response1);

            var response2 = await session.DispatchAsync();
            Assert.AreEqual(message2, response2);
        }

        [TestMethod]
        public async Task Root_Begin_After_End()
        {
            var dialogRoot = MockDialog();

            var message1 = new Connector.Message();
            var message2 = new Connector.Message();

            var queue = new Queue<Connector.Message>(new[] { message1, message2 });

            var session = MakeSession(dialogRoot);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => Task.FromResult(queue.Dequeue()));
            dialogRoot.Setup(d => d.ReplyReceivedAsync(session)).Returns(() => session.EndDialogAsync(dialogRoot.Object, Tasks.Null));

            var response1 = await session.DispatchAsync();
            Assert.AreEqual(message1, response1);

            var response2 = await session.DispatchAsync();
            Assert.AreEqual(message2, response2);
        }
    }

    [TestClass]
    public sealed class DialogFlowTests_EndDialog : DialogFlowTests
    {
        [TestMethod]
        public async Task Leaf_Returns_InBegin_ToRoot()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var expected = Task.FromResult(new object());

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, expected));

            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is<Task<object>>(actual => actual.Result == expected.Result)), Times.Once);
        }

        [TestMethod]
        public async Task Leaf_Returns_InReply_To_Root()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var expected = Task.FromResult(new object());

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.ReplyReceivedAsync(session)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, expected));

            await session.DispatchAsync();
            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is<Task<object>>(actual => actual.Result == expected.Result)), Times.Once);
        }

        [TestMethod]
        public async Task Leaf_Returns_Cancel_ToRoot()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var expected = Tasks.Cancelled;

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, expected));

            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is<Task<object>>(actual => actual.IsCanceled)), Times.Once);
        }

        [TestMethod]
        public async Task Leaf_Returns_Error_ToRoot()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, MockException.Task));

            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is(ExceptionOfType<MockException>())), Times.Once);
        }
    }

    [TestClass]
    public sealed class DialogFlowTests_EndDialog_InvalidTasks : DialogFlowTests
    {
        [TestMethod]
        public async Task Leaf_Returns_InBegin_ToRoot_IncompleteTask()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var expected = new TaskCompletionSource<object>();

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, expected.Task));

            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is(ExceptionOfType<TaskNotCompletedException>())), Times.Once);
        }

        [TestMethod]
        public async Task Leaf_Returns_InReply_ToRoot_IncompleteTask()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var expected = new TaskCompletionSource<object>();

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.ReplyReceivedAsync(session)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, expected.Task));

            await session.DispatchAsync();
            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is(ExceptionOfType<TaskNotCompletedException>())), Times.Once);
        }
    }

    [TestClass]
    public sealed class DialogFlowTests_ThrowsExceptions : DialogFlowTests
    {
        [TestMethod]
        public async Task Root_ThrowsException_InBegin_ToUser()
        {
            var dialogRoot = MockDialog();

            var session = MakeSession(dialogRoot);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Throws<MockException>();

            var taskResponse = session.DispatchAsync();
            await AssertExceptionAsync(taskResponse);
        }

        [TestMethod]
        public async Task Root_ThrowsException_InBegin_ToUser_CanRestart()
        {
            var dialogRoot = MockDialog();

            var session = MakeSession(dialogRoot);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Throws<MockException>();

            var taskResponse = session.DispatchAsync();
            await AssertExceptionAsync(taskResponse);

            var expected = new Connector.Message();
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).ReturnsAsync(expected);

            var actual = await session.DispatchAsync();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public async Task Root_ThrowsException_InReply_ToUser()
        {
            var dialogRoot = MockDialog();

            var session = MakeSession(dialogRoot);

            var expected = new Connector.Message();

            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).ReturnsAsync(expected);
            dialogRoot.Setup(d => d.ReplyReceivedAsync(session)).Throws<MockException>();

            var actual = await session.DispatchAsync();
            Assert.AreEqual(expected, actual);

            var taskResponse = session.DispatchAsync();
            await AssertExceptionAsync(taskResponse);
        }

        [TestMethod]
        public async Task Root_ThrowsException_InReply_ToUser_CanRestart()
        {
            var dialogRoot = MockDialog();

            var session = MakeSession(dialogRoot);

            var expectedBegin = new Connector.Message();

            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).ReturnsAsync(expectedBegin);
            dialogRoot.Setup(d => d.ReplyReceivedAsync(session)).Throws<MockException>();

            {
                var actualBegin = await session.DispatchAsync();
                Assert.AreEqual(expectedBegin, actualBegin);

                var taskResponse = session.DispatchAsync();
                await AssertExceptionAsync(taskResponse);
            }

            {
                var expectedReply = new Connector.Message();
                dialogRoot.Setup(d => d.ReplyReceivedAsync(session)).ReturnsAsync(expectedReply);

                var actualBegin = await session.DispatchAsync();
                Assert.AreEqual(expectedBegin, actualBegin);

                var actualReply = await session.DispatchAsync();
                Assert.AreEqual(expectedReply, actualReply);
            }
        }

        [TestMethod]
        public async Task Root_ThrowsException_InResume_ToUser()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogRoot.Setup(d => d.DialogResumedAsync(session, Tasks.Null)).Throws<MockException>();

            var taskResponse = session.DispatchAsync();
            await AssertExceptionAsync(taskResponse);
        }

        [TestMethod]
        public async Task Root_ThrowsException_InResume_ToUser_CanRestart()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogRoot.Setup(d => d.DialogResumedAsync(session, Tasks.Null)).Throws<MockException>();

            var taskResponse = session.DispatchAsync();
            await AssertExceptionAsync(taskResponse);

            var expectedBegin = new Connector.Message();
            dialogRoot.Setup(d => d.DialogResumedAsync(session, Tasks.Null)).ReturnsAsync(expectedBegin);

            var actualBegin = await session.DispatchAsync();
            Assert.AreEqual(expectedBegin, actualBegin);
        }

        [TestMethod]
        public async Task Leaf_ThrowsException_InBegin_ToRoot()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Throws<MockException>();

            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is(ExceptionOfType<MockException>())), Times.Once);
        }

        [TestMethod]
        public async Task Leaf_ThrowsException_InReply_ToRoot()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.ReplyReceivedAsync(session)).Throws<MockException>();

            await session.DispatchAsync();
            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is(ExceptionOfType<MockException>())), Times.Once);
        }

        [TestMethod]
        public async Task Node_ThrowsException_InResume_ToRoot()
        {
            var dialogRoot = MockDialog("root");
            var dialogNode = MockDialog("node");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot, dialogNode, dialogLeaf);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogNode.Object, Tasks.Null));
            dialogNode.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogLeaf.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.EndDialogAsync(dialogLeaf.Object, Tasks.Null));
            dialogNode.Setup(d => d.DialogResumedAsync(session, Tasks.Null)).Throws<MockException>();

            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is(ExceptionOfType<MockException>())), Times.Once);
        }
    }

    [TestClass]
    public sealed class DialogFlowTests_InvalidSession : DialogFlowTests
    {
        [TestMethod]
        public async Task Root_InvalidSession_ToUser()
        {
            var dialogRoot = MockDialog("root");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));

            var taskResponse = session.DispatchAsync();
            await AssertExceptionAsync(taskResponse, typeof(InvalidSessionException));
        }

        [TestMethod]
        public async Task Leaf_InvalidSession_ToRoot()
        {
            var dialogRoot = MockDialog("root");
            var dialogNode = MockDialog("node");
            var dialogLeaf = MockDialog("leaf");

            var session = MakeSession(dialogRoot, dialogNode);
            dialogRoot.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogNode.Object, Tasks.Null));
            dialogNode.Setup(d => d.BeginAsync(session, Tasks.Null)).Returns(() => session.BeginDialogAsync(dialogLeaf.Object, Tasks.Null));

            await session.DispatchAsync();

            dialogRoot.Verify(d => d.DialogResumedAsync(session, It.Is(ExceptionOfType<InvalidSessionException>())), Times.Once);
        }
    }
}
