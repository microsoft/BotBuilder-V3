using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    public class ConversationTests : DialogTestBase
    {
        [TestMethod]
        public async Task UseTableStorage_Test()
        {
            System.Environment.SetEnvironmentVariable(AppSettingKeys.UseTableStorage, true.ToString());
            bool shouldUse = false;
            Assert.IsTrue(bool.TryParse(Utils.GetAppSetting(AppSettingKeys.UseTableStorage), out shouldUse) && shouldUse);

            var echo = Chain.PostToChain().Select(msg => $"echo: {msg.Text}").PostToUser().Loop();

            using (var container = Build(Options.ResolveDialogFromContainer))
            {
                var builder = new ContainerBuilder();
                builder.RegisterModule(new ConversationModule());
                builder
                    .RegisterInstance(echo)
                    .As<IDialog<object>>();

                builder.Update(container);


                var toBot = MakeTestMessage();

                toBot.Text = "hello";

                using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, CancellationToken.None);
                }


                var queue = container.Resolve<Queue<IMessageActivity>>();
                Assert.AreEqual("echo: hello", queue.Dequeue().Text);

                IBotDataStore<BotData> tableStore = container.Resolve<TableBotDataStore>();
                var privateConversationData = await tableStore.LoadAsync(Address.FromActivity(toBot),
                    BotStoreType.BotPrivateConversationData, CancellationToken.None);
                Assert.IsNotNull(privateConversationData.Data);
            }
        }
    }
}
