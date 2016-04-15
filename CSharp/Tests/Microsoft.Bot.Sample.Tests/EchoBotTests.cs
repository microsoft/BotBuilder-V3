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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Sample.EchoBot;

namespace Microsoft.Bot.Sample.Tests
{
    [TestClass]
    public class EchoBotTests
    {
        [TestMethod]
        public async Task EchoDialogFlow()
        {
            await EchoDialogFlow(new EchoDialog());
        }

        [TestMethod]
        public async Task EchoCommandDialogFlow()
        {
            await EchoDialogFlow(EchoCommandDialog.dialog);
        }

        [TestMethod]
        public async Task EchoChainDialogFlow()
        {
            await EchoDialogFlow(EchoChainDialog.dialog);
        }

        private async Task EchoDialogFlow<T>(IDialog<T> echoDialog)
        {
            // arrange
            var toBot = new Message()
            {
                ConversationId = Guid.NewGuid().ToString(),
                Text = "Test"
            };

            Func<IDialog<T>> MakeRoot = () => echoDialog;

            // act: sending the message
            var toUser = await Conversation.SendAsync(toBot, MakeRoot);

            // assert: check if the dialog returned the right response
            Assert.IsTrue(toUser.Text.StartsWith("1"));
            Assert.IsTrue(toUser.Text.Contains("Test"));

            // act: send the message 10 times
            for(int i = 0; i < 10; i++)
            {
                // pretend we're the intercom switch, and copy the bot data from message to message
                toBot = toUser;

                // post the message
                toUser = await Conversation.SendAsync(toBot, MakeRoot);
            }

            // assert: check the counter at the end
            Assert.IsTrue(toUser.Text.StartsWith("11"));

            // act: send the reset
            toBot = toUser;
            toBot.Text = "reset";
            toUser = await Conversation.SendAsync(toBot, MakeRoot);

            // assert: verify confirmation
            Assert.IsTrue(toUser.Text.ToLower().Contains("are you sure"));

            //send yes as reply
            toBot = toUser;
            toBot.Text = "yes";
            toUser = await Conversation.SendAsync(toBot, MakeRoot);
            Assert.IsTrue(toUser.Text.ToLower().Contains("reset count"));

            //send a random message and check count
            toBot = toUser;
            toBot.Text = "test";
            toUser = await Conversation.SendAsync(toBot, MakeRoot);
            Assert.IsTrue(toUser.Text.StartsWith("1"));
        }
    }
}