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
using System.Collections.Concurrent;

namespace Microsoft.Bot.Builder.Tests
{
    public class MockConnectorFactory : IConnectorClientFactory
    {
        protected readonly ConcurrentDictionary<string, BotData> DataStore = new ConcurrentDictionary<string, BotData>();
        public StateClient StateClient;
        public IBotIdResolver botIdResolver;

        public MockConnectorFactory(IBotIdResolver botIdResolver)
        {
            SetField.NotNull(out this.botIdResolver, nameof(botIdResolver), botIdResolver);
        }

        public IConnectorClient MakeConnectorClient()
        {
            var client = new Mock<ConnectorClient>();
            client.CallBase = true;
            return client.Object;
        }

        public IStateClient MakeStateClient()
        {
            if(this.StateClient == null)
            {
                this.StateClient = MockIBots(this).Object;
            }
            return this.StateClient;
        }
        
        public string GetBotDataKey(string botId, string userId = null, string conversationId = null)
        {
            string key = null;
            if (!String.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(conversationId))
                key = $"{botId}:userconversation:{userId}{conversationId}";
            else if (!String.IsNullOrEmpty(userId))
                key = $"{botId}:user:{userId}";
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

        public Mock<StateClient> MockIBots(MockConnectorFactory mockConnectorFactory)
        {
            var botsClient = new Moq.Mock<StateClient>(MockBehavior.Loose);

            botsClient.Setup(d => d.BotState.SetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, data, headers, token) => {
                    return mockConnectorFactory.UpsertData(botIdResolver.BotId, null, $"{channelId}-{conversationId}", data);
                });

            botsClient.Setup(d => d.BotState.GetConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, headers, token) => {
                    return mockConnectorFactory.GetData(botIdResolver.BotId, null, $"{channelId}-{conversationId}");
                });


            botsClient.Setup(d => d.BotState.SetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
              .Returns<string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (channelId, userId, data, headers, token) => {
                  return mockConnectorFactory.UpsertData(botIdResolver.BotId, $"{channelId}-{userId}", null, data);
              });

            botsClient.Setup(d => d.BotState.GetUserDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Returns<string, string, Dictionary<string, List<string>>, CancellationToken>(async (channelId, userId, headers, token) => {
                    return mockConnectorFactory.GetData(botIdResolver.BotId, $"{channelId}-{userId}", null);
                });

            botsClient.Setup(d => d.BotState.SetPrivateConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BotData>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
             .Returns<string, string, string, BotData, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, userId, data, headers, token) => {
                 return mockConnectorFactory.UpsertData(botIdResolver.BotId, $"{channelId}-{userId}", $"{channelId}-{conversationId}", data);
             });

            botsClient.Setup(d => d.BotState.GetPrivateConversationDataWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
             .Returns<string, string, string, Dictionary<string, List<string>>, CancellationToken>(async (channelId, conversationId, userId, headers, token) => {
                 return mockConnectorFactory.GetData(botIdResolver.BotId, $"{channelId}-{userId}", $"{channelId}-{conversationId}");
             });

            return botsClient;
        }
    }

    public abstract class ConversationTestBase
    {
        [Flags]
        public enum Options {None, InMemoryBotDataStore };
        
        public static IContainer Build(Options options, MockConnectorFactory mockConnectorFactory, params object[] singletons)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule_MakeRoot());

            builder
                .Register((c, p) => mockConnectorFactory)
                    .As<IConnectorClientFactory>()
                    .InstancePerLifetimeScope();

            builder
                .Register(c => new BotIdResolver("testBot"))
                .As<IBotIdResolver>()
                .SingleInstance();

            var r =
              builder
              .Register<Queue<IMessageActivity>>(c => new Queue<IMessageActivity>())
              .AsSelf()
              .InstancePerLifetimeScope();
            
            builder
                .RegisterType<BotToUserQueue>()
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            if (options.HasFlag(Options.InMemoryBotDataStore))
            {
                //Note: memory store will be single instance for the bot
                builder.RegisterType<InMemoryDataStore>()
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .SingleInstance();
            }

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
        public async Task InMemoryBotDataStoreTest()
        {
            var chain = Chain.PostToChain().Select(m => m.Text).ContinueWith<string, string>(async (context, result) =>
                {
                    int t = 0;
                    context.UserData.TryGetValue("count", out t);
                    if (t > 0)
                    {
                        int value; 
                        Assert.IsTrue(context.ConversationData.TryGetValue("conversation", out value));
                        Assert.AreEqual(t-1, value);
                        Assert.IsTrue(context.UserData.TryGetValue("user", out value));
                        Assert.AreEqual(t+1, value);
                        Assert.IsTrue(context.PrivateConversationData.TryGetValue("PrivateConversationData", out value));
                        Assert.AreEqual(t + 2, value);
                    }

                    context.ConversationData.SetValue("conversation", t);
                    context.UserData.SetValue("user", t + 2);
                    context.PrivateConversationData.SetValue("PrivateConversationData", t + 3);
                    context.UserData.SetValue("count", ++t);
                    return Chain.Return($"{t}:{await result}");
                }).PostToUser();
            Func<IDialog<object>> MakeRoot = () => chain;

            using (new FiberTestBase.ResolveMoqAssembly(chain))
            using (var container = Build(Options.InMemoryBotDataStore, new MockConnectorFactory(new BotIdResolver("testBot")), chain))
            {
                var msg = DialogTestBase.MakeTestMessage();
                msg.Text = "test";
                using (var scope = DialogModule.BeginLifetimeScope(container, msg))
                {   
                    scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
                    
                    await Conversation.SendAsync(scope, msg);
                    var reply = scope.Resolve<Queue<IMessageActivity>>().Dequeue();
                    Assert.AreEqual("1:test", reply.Text);
                    var store = scope.Resolve<CachingBotDataStore_LastWriteWins>();
                    Assert.AreEqual(0, store.cache.Count);
                    var dataStore = scope.Resolve<InMemoryDataStore>();
                    Assert.AreEqual(3, dataStore.store.Count);
                }

                for (int i = 0; i < 10; i++)
                {
                    using (var scope = DialogModule.BeginLifetimeScope(container, msg))
                    {
                        scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));
                        await Conversation.SendAsync(scope, msg);
                        var reply = scope.Resolve<Queue<IMessageActivity>>().Dequeue();
                        Assert.AreEqual($"{i+2}:test", reply.Text);
                        var store = scope.Resolve<CachingBotDataStore_LastWriteWins>();
                        Assert.AreEqual(0, store.cache.Count);
                        var dataStore = scope.Resolve<InMemoryDataStore>();
                        Assert.AreEqual(3, dataStore.store.Count);
                        string val = string.Empty;
                        Assert.IsTrue(scope.Resolve<IBotData>().PrivateConversationData.TryGetValue(DialogModule.BlobKey, out val));
                        Assert.AreNotEqual(string.Empty, val);
                    }
                }
            }
        }

        [TestMethod]
        public async Task SendResumeAsyncTest()
        {
            var chain = Chain.PostToChain().Select(m => m.Text).Switch(
                new RegexCase<IDialog<string>>(new Regex("^resume"), (context, data) => { context.UserData.SetValue("resume", true); return Chain.Return("resumed!"); }),
                new DefaultCase<string, IDialog<string>>((context, data) => { return Chain.Return(data); })).Unwrap().PostToUser();

            using (new FiberTestBase.ResolveMoqAssembly(chain))
            using (var container = Build(Options.InMemoryBotDataStore, new MockConnectorFactory(new BotIdResolver("testBot")), chain))
            {
                var msg = DialogTestBase.MakeTestMessage();
                msg.Text = "testMsg";
                
                using (var scope = DialogModule.BeginLifetimeScope(container, msg))
                {
                    Func<IDialog<object>> MakeRoot = () => chain;
                    scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));

                    await Conversation.SendAsync(scope, msg);
                    var reply = scope.Resolve<Queue<IMessageActivity>>().Dequeue();
                }

                var resumptionCookie = new ResumptionCookie(msg);
                var continuationMessage = resumptionCookie.GetMessage(); 
                using (var scope = DialogModule.BeginLifetimeScope(container, continuationMessage))
                {
                    Func<IDialog<object>> MakeRoot = () => { throw new InvalidOperationException(); };
                    scope.Resolve<Func<IDialog<object>>>(TypedParameter.From(MakeRoot));

                    await Conversation.ResumeAsync(scope, continuationMessage, new Activity { Text = "resume" });
                    var reply = scope.Resolve<Queue<IMessageActivity>>().Dequeue();
                    Assert.AreEqual("resumed!", reply.Text);

                    var botData = scope.Resolve<IBotData>();
                    await botData.LoadAsync(default(CancellationToken));
                    Assert.IsTrue(botData.UserData.Get<bool>("resume"));
                }
            }
        }
    }
}
