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
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace Microsoft.Bot.Builder.Azure
{
    /// <summary>
    /// Log conversation activities to Azure Table Storage.
    /// </summary>
    public class TableLogger : IActivityLogger, IActivitySource, IActivityManager
    {
        private class ActivityEntity : TableEntity
        {
            public ActivityEntity(IActivity activity)
            {
                PartitionKey = GeneratePartitionKey(activity.ChannelId, activity.Conversation.Id);
                RowKey = GenerateRowKey(activity.Timestamp.Value);
                Activity = JsonConvert.SerializeObject(activity);
                Version = 3.0;
            }

            public double Version { get; set; }

            public string Activity { get; set; }

            public static string GeneratePartitionKey(string channelId, string conversationId)
            {
                return $"{channelId}|{conversationId}";
            }

            public static string GenerateRowKey(DateTime timestamp)
            {
                return $"{DateTime.MaxValue.Ticks - timestamp.Ticks:D19}";
            }
        }

        private CloudTable _table = null;

        /// <summary>
        /// Create a table storage logger.
        /// </summary>
        /// <param name="table">Table stroage to use for storing activities.</param>
        public TableLogger(CloudTable table)
        {
            _table = table;
        }

        /// <summary>
        /// Log activity to table storage.
        /// </summary>
        /// <param name="activity">Activity to log.</param>
        Task IActivityLogger.LogAsync(IActivity activity)
        {
            if (!activity.Timestamp.HasValue)
            {
                activity.Timestamp = DateTime.UtcNow;
            }
            return Write(_table, activity);
        }

        /// <summary>
        /// Produce an enumeration over conversation in time reversed order.
        /// </summary>
        /// <param name="channelId">Channel where conversation happened.</param>
        /// <param name="conversationId">Conversation within the channel.</param>
        /// <param name="max">Maximum number of activities to return.</param>
        /// <param name="oldest">Earliest time to include.</param>
        /// <returns>Enumeration over the recorded activities.</returns>
        Task<IEnumerable<IActivity>> IActivitySource.Activities(string channelId, string conversationId, int? max, DateTime oldest)
        {
            var query = new TableQuery();
            var pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ActivityEntity.GeneratePartitionKey(channelId, conversationId));
            if (oldest != default(DateTime))
            {
                var rowKey = ActivityEntity.GenerateRowKey(oldest);
                query = query.Where(TableQuery.CombineFilters(pkFilter, TableOperators.And, TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, rowKey)));
            }
            else
            {
                query = query.Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ActivityEntity.GeneratePartitionKey(channelId, conversationId)));
            }
            query = query.Take(max);
            return Task.FromResult(_table.ExecuteQuery<IActivity>(query,
                (partitionKey, rowKey, timestamp, properties, etag) => JsonConvert.DeserializeObject<Activity>(properties["Activity"].StringValue)));
        }

        /// <summary>
        /// Delete a specific conversation.
        /// </summary>
        /// <param name="channelId">Channel identifier.</param>
        /// <param name="conversationId">Conversation identifier.</param>
        /// <returns>Task.</returns>
        async Task IActivityManager.DeleteConversation(string channelId, string conversationId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Delete any conversation records older than <paramref name="oldest"/>.
        /// </summary>
        /// <param name="oldest">Maximum timespan from now to remember.</param>
        async Task IActivityManager.DeleteBefore(DateTime oldest)
        {
            var rowKey = ActivityEntity.GenerateRowKey(oldest);
            var query = new TableQuery<TableEntity>().Select(new string[] { "PartitionKey", "RowKey" });
            TableContinuationToken continuationToken = null;
            do
            {
                var results = await _table.ExecuteQuerySegmentedAsync(query, continuationToken);
                var partitionKey = string.Empty;
                var batch = new TableBatchOperation();
                foreach (var result in (from r in results where string.Compare(r.RowKey, rowKey, StringComparison.Ordinal) > 0 select r))
                {
                    if (result.PartitionKey == partitionKey)
                    {
                        if (batch.Count == 100)
                        {
                            await _table.ExecuteBatchAsync(batch);
                            batch = new TableBatchOperation();
                        }
                    }
                    else
                    {
                        if (batch.Count > 0)
                        {
                            await _table.ExecuteBatchAsync(batch);
                            batch = new TableBatchOperation();
                        }
                        partitionKey = result.PartitionKey;
                    }
                    batch.Add(TableOperation.Delete(result));
                }
                if (batch.Count > 0)
                {
                    await _table.ExecuteBatchAsync(batch);
                }
                continuationToken = results.ContinuationToken;
            } while (continuationToken != null);
        }

        // Write out activity with auto-incrementing of timestamp for conflicts up to 5 times
        private static Task Write(CloudTable table, IActivity activity, int retriesLeft = 5)
        {
            var insert = TableOperation.Insert(new ActivityEntity(activity));
            return table.ExecuteAsync(insert).ContinueWith(t =>
            {
                if (--retriesLeft > 0 && t.IsFaulted)
                {
                    var response = ((t.Exception.InnerException as StorageException)?.InnerException as System.Net.WebException)?.Response as System.Net.HttpWebResponse;
                    if (response != null && response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        activity.Timestamp = activity.Timestamp.Value.AddTicks(1);
                        return TableLogger.Write(table, activity, retriesLeft);
                    }
                }
                t.Wait();
                return t;
            });
        }
    }

    /// <summary>
    /// Module for registering a LoggerTable.
    /// </summary>
    public class TableLoggerModule : Autofac.Module
    {
        private CloudStorageAccount _account;
        private string _tableName;

        /// <summary>
        /// Create a TableLogger for a particular storage account and table name.
        /// </summary>
        /// <param name="account">Azure storage account to use.</param>
        /// <param name="tableName">Where to log activities.</param>
        public TableLoggerModule(CloudStorageAccount account, string tableName)
        {
            _account = account;
            _tableName = tableName;
        }

        /// <summary>
        /// Update builder with registration for TableLogger.
        /// </summary>
        /// <param name="builder">Builder to use for registration.</param>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterInstance(_account)
                .AsSelf();
            builder.Register(c => c.Resolve<CloudStorageAccount>().CreateCloudTableClient())
                .AsSelf()
                .SingleInstance();
            builder.Register(c =>
            {
                var table = c.Resolve<CloudTableClient>().GetTableReference(_tableName);
                table.CreateIfNotExists();
                return table;
            })
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<TableLogger>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }

    static partial class Extensions
    {
        public static void Forget<T>(this Task<T> task)
        {
        }
    }
}
