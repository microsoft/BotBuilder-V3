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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Bot.Connector;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Rest;
using System.Net.Http;
using System.Net;
using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Dialogs;
using System.Text.RegularExpressions;

namespace Microsoft.Bot.Builder.Tests
{
    public class MockConnectorFactory : IConnectorClientFactory
    {
        protected readonly Dictionary<string, BotData> DataStore = new Dictionary<string, BotData>();

        public IConnectorClient Make()
        {
            var client = new Mock<ConnectorClient>();
            client.Setup(d => d.Bots).Returns(MockIBots(this).Object);
            client.CallBase = true;
            return client.Object;
        }


        public string GetBotDataKey(string botId, string userId = null, string conversationId = null)
        {
            string key = null;
            if (!String.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(conversationId))
                key = $"{botId}:userconversation:{userId}{conversationId}";
            else if (!String.IsNullOrEmpty(userId))
                key = $"user:{userId}";
            else if (!String.IsNullOrEmpty(conversationId))
                key = $"{botId}:conversation:{conversationId}";
            else
                throw new ArgumentNullException("There is no userId or conversationId");
            return key;
        }

        protected HttpOperationResponse<object> UpsertData(string botId, string userId, string conversationId, BotData data)
        {
            var _result = new HttpOperationResponse<object>();
            data.ETag = "MockedDataStore";
            DataStore[GetBotDataKey(botId, userId, conversationId)] = data;
            _result.Body = data;
            _result.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            return _result;
        }

        protected HttpOperationResponse<object> GetData(string botId, string userId, string conversationId)
        {
            var _result = new HttpOperationResponse<object>();
            BotData data;
            DataStore.TryGetValue(GetBotDataKey(botId, userId, conversationId), out data);
            _result.Body = data;
            _result.Response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            return _result;

        }

        public static Mock<IBots> MockIBots(MockConnectorFactory mockConnectorFactory)
        {
            var botsClient = new Moq.Mock<IBots>(MockBehavior.Loose);

            botsClient.Setup(d => d.SetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, data, headers, token) => {
                    return mockConnectorFactory.UpsertData(botId, null, conversationId, data);
                });

            botsClient.Setup(d => d.GetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, headers, token) => {
                    return mockConnectorFactory.GetData(botId, null, conversationId);
                });


            botsClient.Setup(d => d.SetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
              .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (botId, userId, data, headers, token) => {
                  return mockConnectorFactory.UpsertData(botId, userId, null, data);
              });

            botsClient.Setup(d => d.GetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (botId, userId, headers, token) => {
                    return mockConnectorFactory.GetData(botId, userId, null);
                });

            botsClient.Setup(d => d.SetPerUserInConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
             .Returns<string, string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, userId, data, headers, token) => {
                 return mockConnectorFactory.UpsertData(botId, userId, conversationId, data);
             });

            botsClient.Setup(d => d.GetPerUserConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
             .Returns<string, string, string, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, userId, headers, token) => {
                 return mockConnectorFactory.GetData(botId, userId, conversationId);
             });

            return botsClient;
        }
    }

    public abstract class ConversationTestBase
    {
        public static IContainer Build(MockConnectorFactory mockConnectorFactory, params object[] singletons)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule_MakeRoot());

            builder
                .Register((c, p) => mockConnectorFactory)
                    .As<IConnectorClientFactory>()
                    .InstancePerLifetimeScope(); 

            foreach (var singleton in singletons)
            {
                builder
                    .Register(c => singleton)
                    .Keyed(FiberModule.Key_DoNotSerialize, singleton.GetType());
            }

            return builder.Build();
        }
    }


    [TestClass]
    public sealed class ConversationTest : ConversationTestBase
    {
        [TestMethod]
        public async Task SendResumeAsyncTest()
        {
            var chain = Chain.PostToChain().Select(m => m.Text).Switch(
                new RegexCase<IDialog<string>>(new Regex("^resume"), (context, data) => { context.UserData.SetValue("resume", true); return Chain.Return("resumed!"); }),
                new DefaultCase<string, IDialog<string>>((context, data) => { return Chain.Return(data); })).Unwrap().PostToUser();

            using (new FiberTestBase.ResolveMoqAssembly(chain))
            using (var container = Build(new MockConnectorFactory(), chain))
            {
                var msg = new Message
                {
                    ConversationId = Guid.NewGuid().ToString(),
                    Text = "testMsg",
                    From = new ChannelAccount { Id = "testUser" },
                    To = new ChannelAccount { Id = "testBot" }
                };

                using (var scope = DialogModule.BeginLifetimeScope(container, msg))
                {
                    Func<IDialog<object>> MakeRoot = () => chain;
                    scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));

                    var reply = await Conversation.SendAsync(scope, msg);

                    // storing data in mocked connector client
                    var client = scope.Resolve<IConnectorClient>();
                    await PersistMessageData(client, msg.To.Id, msg.From.Id, msg.ConversationId, reply);
                }

                var resumptionCookie = new ResumptionCookie(msg);
                var continuationMessage = resumptionCookie.GetMessage(); 
                using (var scope = DialogModule.BeginLifetimeScope(container, continuationMessage))
                {
                    Func<IDialog<object>> MakeRoot = () => { throw new InvalidOperationException(); };
                    scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));

                    var reply = await Conversation.ResumeAsync(scope, continuationMessage, new Message { Text = "resume" });
                    Assert.AreEqual("resumed!", reply.Text);

                    var client = scope.Resolve<IConnectorClient>();
                    await PersistMessageData(client, msg.To.Id, msg.From.Id, msg.ConversationId, reply);
                    Assert.IsTrue(client.Bots.GetUserData(msg.To.Id, msg.From.Id).GetProperty<bool>("resume"));
                    Assert.AreEqual("MockedDataStore", client.Bots.GetUserData(msg.To.Id, msg.From.Id).ETag);
                }
            }
        }

        private async Task PersistMessageData(IConnectorClient client, string botId, string userId, string conversationId, Message msg)
        {
            await client.Bots.SetConversationDataAsync(botId, conversationId, new BotData(msg.BotConversationData));
            await client.Bots.SetUserDataAsync(botId, userId, new BotData(msg.BotUserData));
            await client.Bots.SetPerUserInConversationDataAsync(botId, conversationId, userId, new BotData(msg.BotPerUserInConversationData));
        }
    }
}
