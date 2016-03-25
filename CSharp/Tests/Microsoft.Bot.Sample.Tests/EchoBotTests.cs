using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;

using Microsoft.Bot.Sample.EchoBot;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Sample.Tests
{
    [TestClass]
    public class EchoBotTests
    {
        [TestMethod]
        public async Task EchoDialogFlow()
        {
            // arrange
            var toBot = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = "Test"
            };

            Func<IDialog<object>> MakeRoot = () => new EchoDialog();

            // act: sending the message
            var toUser = await CompositionRoot.SendAsync(toBot, MakeRoot);

            // assert: check if the dialog returned the right response
            Assert.IsTrue(toUser.Text.StartsWith("0"));
            Assert.IsTrue(toUser.Text.Contains("Test"));

            // act: send the message 10 times
            for(int i = 0; i < 10; i++)
            {
                // pretend we're the intercom switch, and copy the bot data from message to message
                toBot = toUser;

                // post the message
                toUser = await CompositionRoot.SendAsync(toBot, MakeRoot);
            }

            // assert: check the counter at the end
            Assert.IsTrue(toUser.Text.StartsWith("10"));

            // act: send the reset
            toBot = toUser;
            toBot.Text = "reset";
            toUser = await CompositionRoot.SendAsync(toBot, MakeRoot);

            // assert: verify confirmation
            Assert.IsTrue(toUser.Text.ToLower().Contains("are you sure"));

            //send yes as reply
            toBot = toUser;
            toBot.Text = "yes";
            toUser = await CompositionRoot.SendAsync(toBot, MakeRoot);
            Assert.IsTrue(toUser.Text.ToLower().Contains("count reset"));

            //send a random message and check count
            toBot = toUser;
            toBot.Text = "test";
            toUser = await CompositionRoot.SendAsync(toBot, MakeRoot);
            Assert.IsTrue(toUser.Text.StartsWith("0"));
        }
    }
}