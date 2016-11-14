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

        public static readonly object Key_DataStore = new object();
        

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
                    .Keyed<IBotDataStore<BotData>>(Key_DataStore)
                    .AsSelf()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new ConnectorStore(c.Resolve<IStateClient>()))
                    .Keyed<IBotDataStore<BotData>>(Key_DataStore)
                    .AsSelf()
                    .InstancePerLifetimeScope();
            }

            builder.Register(c => new CachingBotDataStore(c.ResolveKeyed<IBotDataStore<BotData>>(Key_DataStore),
                        CachingBotDataStoreConsistencyPolicy.LastWriteWins))
                    .As<IBotDataStore<BotData>>()
                    .AsSelf()
                    .InstancePerLifetimeScope();
        }

        private bool ShouldUseTableStorage()
        {
            bool shouldUseTableStorage = false;
            var useTableStore = Utils.GetAppSetting(AppSettingKeys.UseTableStorageForConversationState);
            return bool.TryParse(useTableStore, out shouldUseTableStorage) && shouldUseTableStorage;
        }

        private TableBotDataStore MakeTableBotDataStore()
        {
            var connectionString = Utils.GetAppSetting(AppSettingKeys.TableStorageConnectionString);

            if (!string.IsNullOrEmpty(connectionString))
            {
                return new TableBotDataStore(connectionString);
            }

            // no connection string in application settings but should use table storage flag is set.
            throw new ArgumentException("connection string for table storage is not set in application setting.");
        }
    }
}
