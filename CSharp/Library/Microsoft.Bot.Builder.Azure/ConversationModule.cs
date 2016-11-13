using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Autofac module for updating <see cref="Dialogs.Conversation"/> components.
    /// </summary>
    public sealed class ConversationModule : Module
    {
        /// <summary>
        /// Registers dependencies with the <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder"> The container builder.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConnectorStore>()
                .AsSelf()
                .InstancePerLifetimeScope();

            if (ShouldUseTableStorage())
            {

                builder.Register(c => MakeTableBotDataStore())
                    .AsSelf()
                    .SingleInstance();

                builder.Register(c => new CachingBotDataStore(c.Resolve<TableBotDataStore>(),
                        CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }
        }

        private bool ShouldUseTableStorage()
        {
            bool shouldUseTableStorage = false;
            var useTableStore = Utils.GetAppSetting(AppSettingKeys.UseTableStorageForConversationState);
            return bool.TryParse(useTableStore, out shouldUseTableStorage) && shouldUseTableStorage;
        }

        private TableBotDataStore MakeTableBotDataStore()
        {
            TableBotDataStore tableDataStore = default(TableBotDataStore);
            var connectionString = Utils.GetAppSetting(AppSettingKeys.TableStorageConnectionString);

            if (!string.IsNullOrEmpty(connectionString))
            {
                tableDataStore = new TableBotDataStore(connectionString);
            }
            else
            {
                // if no connection string try to use the storage emulator
                tableDataStore = new TableBotDataStore(CloudStorageAccount.DevelopmentStorageAccount);
            }

            return tableDataStore;
        }
    }
}
