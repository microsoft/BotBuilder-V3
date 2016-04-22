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
    public abstract class ConversationTestBase : IConnectorClientFactory
    {
        protected readonly Dictionary<string, BotData> DataStore = new Dictionary<string, BotData>();

        public static IContainer Build(ConversationTestBase testContext, params object[] singletons)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule());

            builder
                .RegisterType<BotToUserQueue>()
                .Keyed<IBotToUser>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            builder
                .Register((c, p) => testContext)
                    .As<IConnectorClientFactory>()
                    .InstancePerLifetimeScope();

            builder
                .Register((c, p) => new SendLastInline_BotToUser(p.TypedAs<Message>(), c.Resolve<IConnectorClient>()))
                .Keyed<IBotToUser>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            foreach (var singleton in singletons)
            {
                builder
                    .Register(c => singleton)
                    .Keyed<object>(FiberModule.Key_DoNotSerialize);
            }

            return builder.Build();
        }

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

        public static Mock<IBots> MockIBots(ConversationTestBase testContext)
        {
            var botsClient = new Moq.Mock<IBots>(MockBehavior.Loose);

            botsClient.Setup(d => d.SetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, data, headers, token) => {
                    return testContext.UpsertData(botId, null, conversationId, data);
                });

            botsClient.Setup(d => d.GetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, headers, token) => {
                    return testContext.GetData(botId, null, conversationId);
                });


            botsClient.Setup(d => d.SetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
              .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (botId, userId, data, headers, token) => {
                  return testContext.UpsertData(botId, userId, null, data);
              });

            botsClient.Setup(d => d.GetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (botId, userId, headers, token) => {
                    return testContext.GetData(botId, userId, null);
                });

            botsClient.Setup(d => d.SetPerUserInConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
             .Returns<string, string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, userId, data, headers, token) => {
                 return testContext.UpsertData(botId, userId, conversationId, data);
             });

            botsClient.Setup(d => d.GetPerUserConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
             .Returns<string, string, string, Dictionary<string, List<string>>, CancellationToken>(async (botId, conversationId, userId, headers, token) => {
                 return testContext.GetData(botId, userId, conversationId);
             });

            return botsClient;
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
            using (var container = Build(this, chain))
            {
                var msg = new Message
                {
                    ConversationId = Guid.NewGuid().ToString(),
                    Text = "testMsg",
                    From = new ChannelAccount { Id = "testUser" },
                    To = new ChannelAccount { Id = "testBot" }
                };

                using (var scope = container.BeginLifetimeScope())
                {
                    var reply = await Conversation.SendAsync(scope, msg, () => chain);

                    // storing data in mocked connector client
                    var client = scope.Resolve<IConnectorClient>();
                    await PersistMessageData(client, msg.To.Id, msg.From.Id, msg.ConversationId, reply);
                }

                using (var scope = container.BeginLifetimeScope())
                {
                    var reply = await Conversation.ResumeAsync(scope, msg.To.Id, msg.From.Id, msg.ConversationId, new Message { Text = "resume" });
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
