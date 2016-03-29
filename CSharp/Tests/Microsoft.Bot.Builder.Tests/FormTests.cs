using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.FormFlow;

namespace Microsoft.Bot.Builder.Tests
{
#pragma warning disable CS1998

    [TestClass]
    public sealed class FormTests
    {
        public interface IFormTarget
        {
            int Integer { get; set; }
            string Text { get; set; }
            float Float { get; set; }
        }

        public static void AssertMentions(Message toUser, string text)
        {
            var index = toUser.Text.IndexOf(text, StringComparison.OrdinalIgnoreCase);
            Assert.IsTrue(index >= 0);
        }

        [TestMethod]
        public async Task Can_Fill_In_Scalar_Types()
        {
            var mock = new Mock<IFormTarget>();
            mock.SetupAllProperties();

            Func<IDialog> MakeRoot = () => new FormDialog<IFormTarget>(mock.Object);

            // arrange
            var toBot = new Message() { ConversationId = Guid.NewGuid().ToString() };

            // act
            var toUser = await Conversation.SendAsync(toBot, MakeRoot, default(CancellationToken), mock.Object);

            // assert
            AssertMentions(toUser, nameof(mock.Object.Integer));


            // arrange
            toBot.Text = "3";

            // act
            toUser = await Conversation.SendAsync(toBot, MakeRoot, default(CancellationToken), mock.Object);

            // assert
            AssertMentions(toUser, nameof(mock.Object.Text));


            // arrange
            toBot.Text = "words";

            // act
            toUser = await Conversation.SendAsync(toBot, MakeRoot, default(CancellationToken), mock.Object);

            // assert
            AssertMentions(toUser, nameof(mock.Object.Float));


            // arrange
            toBot.Text = "3.5";

            // act
            toUser = await Conversation.SendAsync(toBot, MakeRoot, default(CancellationToken), mock.Object);

            // assert
            AssertMentions(toUser, nameof(mock.Object.Float));

            mock.VerifyAll();
        }
    }
}
