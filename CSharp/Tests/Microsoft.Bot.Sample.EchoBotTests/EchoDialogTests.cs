using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Sample.EchoBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;


namespace Microsoft.Bot.Sample.EchoBot.Tests
{
    [TestClass]
    public class EchoDialogTests
    {
        [TestMethod]
        public async Task EchoDialogTest()
        {
            var echo = EchoDialog.Instance;
            var message = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = "Test"
            };

            //sending the message
            var dialogs = new DialogCollection().Add(echo);
            var reply = await ConsoleSession.MessageReceivedAsync(message, dialogs, echo);
            Assert.AreEqual(reply.Status, System.Net.HttpStatusCode.OK);
            
            //check if the dialog returned the right response
            Assert.IsTrue(reply.Msg.Text.StartsWith("0"));
            Assert.IsTrue(reply.Msg.Text.Contains("Test"));

            //send the message 10 times and check the counter at the end
            for(int i = 0; i < 10; i++)
            {
                reply = await ConsoleSession.MessageReceivedAsync(message, dialogs, echo);
            }
            Assert.IsTrue(reply.Msg.Text.StartsWith("10"));

            //send the reset
            message.Text = "reset";
            reply = await ConsoleSession.MessageReceivedAsync(message, dialogs, echo);
            Assert.IsTrue(reply.Msg.Text.ToLower().Contains("are you sure"));

            //send yes as reply
            message.Text = "yes";
            reply = await ConsoleSession.MessageReceivedAsync(message, dialogs, echo);
            Assert.IsTrue(reply.Msg.Text.ToLower().Contains("count reset"));

            //send a random message and check count
            message.Text = "test";
            reply = await ConsoleSession.MessageReceivedAsync(message, dialogs, echo);
            Assert.IsTrue(reply.Msg.Text.StartsWith("0"));
        }
    }
}