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
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public sealed class LoggerTests
    {
        private const string _defaultChannel = "channel1";
        private const string _defaultConversation = "conversation1";
        private const string _defaultUser = "user1";
        private const string _defaultBot = "bot1";
        private DateTime _lastActivity = DateTime.UtcNow;

        private IActivity MakeActivity(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "message",
            string channel = _defaultChannel,
            string conversation = _defaultConversation,
            string from = _defaultUser,
            string to = _defaultBot)
        {
            _lastActivity += TimeSpan.FromSeconds(increment);
            return new Activity
            {
                Timestamp = _lastActivity,
                Type = type,
                ChannelId = channel,
                Conversation = new ConversationAccount(id: conversation),
                From = new ChannelAccount(id: from),
                Recipient = new ChannelAccount(id: to),
                Text = text,
                Attachments = attachments
            };
        }

        private IActivity ToUser(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "message",
            string channel = _defaultChannel,
            string conversation = _defaultConversation)
        {
            return MakeActivity(text, attachments, increment, type, channel, conversation, _defaultBot, _defaultUser);
        }

        private IActivity ToBot(
            string text = null,
            IList<Attachment> attachments = null,
            int increment = 1,
            string type = "message",
            string channel = _defaultChannel,
            string conversation = _defaultConversation)
        {
            return MakeActivity(text, attachments, increment, type, channel, conversation, _defaultUser, _defaultBot);
        }

        private IEnumerable<IActivity> Filter(IEnumerable<IActivity> activities, int last, int? max = null, DateTime oldest = default(DateTime))
        {
            return (from activity in activities.Take(last + 1).Reverse() where activity.Timestamp >= oldest select activity).Take(max??int.MaxValue);
        }

        public class CompareActivity : IEqualityComparer<IActivity>
        {
            public bool Equals(IActivity x, IActivity y)
            {
                var m1 = x as IMessageActivity;
                var m2 = y as IMessageActivity;
                return m1.ChannelId == m2.ChannelId
                    && m1.Conversation.Id == m2.Conversation.Id
                    && m1.From.Id == m2.From.Id
                    && m1.Recipient.Id == m2.Recipient.Id
                    && m1.Timestamp == m2.Timestamp
                    && m1.Text == m2.Text
                    // && m1.Attachments == m2.Attachments
                    && m1.Type == m2.Type;
            }

            public int GetHashCode(IActivity obj)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task LogAndReplay()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterModule(new TableLoggerModule(CloudStorageAccount.DevelopmentStorageAccount, "Activities"));
            var container = builder.Build();
            var logger = container.Resolve<IActivityLogger>();
            var source = container.Resolve<IActivitySource>();
            var manager = container.Resolve<IActivityManager>();
            var activities = new List<IActivity>
            {
                ToBot("Hi"),
                ToUser("Welcome to the bot"),
                ToBot("Weather"),
                ToUser("or not"),
                ToBot("sometime later", increment:180)
            };
            var comparator = new CompareActivity();
            for(var i = 0; i < activities.Count; ++i)
            {
                await logger.LogAsync(activities[i]);
                var oldest = _lastActivity.AddSeconds(-30);
                var expected = Filter(activities, i, 2, oldest).ToList();
                var actual = (await source.Activities(_defaultChannel, _defaultConversation, 2, oldest)).ToList();
                Assert.IsTrue(expected.SequenceEqual(actual, comparator));
            }
            var purge = _lastActivity.AddSeconds(-30.0);
            await manager.DeleteBefore(purge);
            var expectedAfter = Filter(activities, activities.Count(), oldest:purge);
            var actualAfter = await source.Activities(_defaultChannel, _defaultConversation);
            Assert.IsTrue(expectedAfter.SequenceEqual(actualAfter, comparator));
        }
    }
}
