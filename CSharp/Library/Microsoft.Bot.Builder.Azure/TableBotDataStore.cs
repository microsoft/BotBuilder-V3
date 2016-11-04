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
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using System.Web;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Bot.Builder.Azure
{
    internal class EntityKey
    {
        public EntityKey(string partition, string row)
        {
            PartitionKey = partition;
            RowKey = row;
        }

        public string PartitionKey { get; private set; }
        public string RowKey { get; private set; }

    }

    internal class BotDataEntity : TableEntity
    {
        private static readonly JsonSerializerSettings serializationSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        };

        public BotDataEntity()
        {
        }

        internal BotDataEntity(string botId, string channelId, string conversationId, string userId, object data)
        {
            this.BotId = botId;
            this.ChannelId = channelId;
            this.ConversationId = conversationId;
            this.UserId = userId;
            this.Data = JsonConvert.SerializeObject(data, serializationSettings);
        }

        internal static EntityKey GetEntityKey(IAddress key, BotStoreType botStoreType)
        {
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    return new EntityKey($"{key.BotId}:{key.ChannelId}:{key.ConversationId}", "conversation");

                case BotStoreType.BotUserData:
                    return new EntityKey($"{key.BotId}:{key.ChannelId}:{key.UserId}", "user");

                case BotStoreType.BotPrivateConversationData:
                    return new EntityKey($"{key.BotId}:{key.ChannelId}:{key.UserId}:{key.ConversationId}", "private");

                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }

        internal ObjectT GetData<ObjectT>()
        {
            return JsonConvert.DeserializeObject<ObjectT>(this.Data);
        }

        internal object GetData()
        {
            return JsonConvert.DeserializeObject(this.Data);
        }

        public string BotId { get; set; }

        public string ChannelId { get; set; }

        public string ConversationId { get; set; }

        public string UserId { get; set; }

        public string Data { get; set; }
    }

    /// <summary>
    /// IBotDataStore<> Implementation using Azure Storage Table 
    /// </summary>
    public class TableBotDataStore : IBotDataStore<BotData>
    {
        private CloudTable _table;
        private static HashSet<string> _checkedTables = new HashSet<string>();

        public TableBotDataStore(string connectionString, string tableName = "botdata")
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(tableName);

            lock (_checkedTables)
            {
                if (!_checkedTables.Contains(tableName))
                {
                    _table.CreateIfNotExists();
                    _checkedTables.Add(tableName);
                }
            }
        }

        public TableBotDataStore(CloudTable table)
        {
            _table = table;
        }

        public CloudTable Table {  get { return _table; } set { _table = value; } }

        async Task<BotData> IBotDataStore<BotData>.LoadAsync(IAddress key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            var entityKey = BotDataEntity.GetEntityKey(key, botStoreType);
            try
            {
                var result = await _table.ExecuteAsync(TableOperation.Retrieve<BotDataEntity>(entityKey.PartitionKey, entityKey.RowKey));
                BotDataEntity entity = result.Result as BotDataEntity;
                if (entity == null)
                    return new BotData(String.Empty, null);
                return new BotData(entity.ETag, entity.GetData());
            }
            catch (StorageException err)
            {
                switch ((HttpStatusCode)err.RequestInformation.HttpStatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return new BotData(String.Empty, null);
                    default:
                        throw new HttpException(err.RequestInformation.HttpStatusCode, err.RequestInformation.HttpStatusMessage);
                }
            }
        }

        async Task IBotDataStore<BotData>.SaveAsync(IAddress key, BotStoreType botStoreType, BotData botData, CancellationToken cancellationToken)
        {
            var entityKey = BotDataEntity.GetEntityKey(key, botStoreType);
            BotDataEntity entity = new BotDataEntity(key.BotId, key.ChannelId, key.ConversationId, key.UserId, botData.Data)
            {
                ETag = botData.ETag
            };
            entity.PartitionKey = entityKey.PartitionKey;
            entity.RowKey = entityKey.RowKey;

            try
            {
                if (String.IsNullOrEmpty(entity.ETag))
                    await _table.ExecuteAsync(TableOperation.Insert(entity));
                else if (entity.ETag == "*")
                {
                    if (botData.Data != null)
                        await _table.ExecuteAsync(TableOperation.InsertOrReplace(entity));
                    else
                        await _table.ExecuteAsync(TableOperation.Delete(entity));
                }
                else
                {
                    if (botData.Data != null)
                        await _table.ExecuteAsync(TableOperation.Replace(entity));
                    else
                        await _table.ExecuteAsync(TableOperation.Delete(entity));
                }
            }
            catch (StorageException err)
            {
                if ((HttpStatusCode)err.RequestInformation.HttpStatusCode == HttpStatusCode.Conflict)
                    throw new HttpException((int)HttpStatusCode.PreconditionFailed, err.RequestInformation.HttpStatusMessage);

                throw new HttpException(err.RequestInformation.HttpStatusCode, err.RequestInformation.HttpStatusMessage);
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(IAddress key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }

    }
}
