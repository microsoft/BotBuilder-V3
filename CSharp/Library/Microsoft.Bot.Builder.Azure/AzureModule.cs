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

using Autofac;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Autofac module for azure bot components.
    /// </summary>
    public sealed class AzureModule : Module
    {

        /// <summary>
        /// The key for data store register with the container.
        /// </summary>
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

            builder.Register(c =>
                {
                    var activity = c.Resolve<IActivity>();
                    if (activity.ChannelId == "emulator")
                    {
                        // for emulator we should use serviceUri of the emulator for storage
                        return new StateClient(new Uri(activity.ServiceUrl));
                    }

                    MicrosoftAppCredentials.TrustServiceUrl(AzureBot.stateApi.Value, DateTime.MaxValue);
                    return new StateClient(new Uri(AzureBot.stateApi.Value));
                })
            .As<IStateClient>()
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
