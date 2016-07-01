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

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public enum BotStoreType
    {
        BotConversationData,
        BotPrivateConversationData,
        BotUserData
    }

    public class BotDataKey
    {
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public string BotId { get; set; }

        public string ChannelId { get; set;}

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is BotDataKey))
            {
                return false;
            }
            else
            {
                var otherKey = (BotDataKey)obj;
                return otherKey.UserId == this.UserId &&
                    otherKey.ConversationId == this.ConversationId &&
                    otherKey.ChannelId == this.ChannelId &&
                    otherKey.BotId == this.BotId;
            }
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode() +
                ConversationId.GetHashCode() +
                BotId.GetHashCode() +
                ChannelId.GetHashCode(); 
        }
    }
    
    public interface IBotDataStore<T>
    {
        Task<T> LoadAsync(BotDataKey key, BotStoreType botStoreType, CancellationToken cancellationToken);
        Task SaveAsync(BotDataKey key, BotStoreType botStoreType, T data, CancellationToken cancellationToken);
        Task<bool> FlushAsync(BotDataKey key, CancellationToken cancellationToken);
    }

    public interface IBotDataStore : IBotDataStore<object>
    {

    }

    public class InMemoryDataStore : IBotDataStore<BotData>
    {
        internal readonly ConcurrentDictionary<string, string> store = new ConcurrentDictionary<string, string>();
        private readonly Dictionary<BotStoreType, object> locks = new Dictionary<BotStoreType, object>()
        {
            { BotStoreType.BotConversationData, new object() },
            { BotStoreType.BotPrivateConversationData, new object() },
            { BotStoreType.BotUserData, new object() }
        };
        
        async Task<BotData> IBotDataStore<BotData>.LoadAsync(BotDataKey key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            string serializedData;
            serializedData = store.GetOrAdd(GetKey(key, botStoreType), 
                                            dictionaryKey => Serialize(new BotData { ETag = DateTime.UtcNow.ToString() }));
            return Deserialize(serializedData);
        }

        async Task IBotDataStore<BotData>.SaveAsync(BotDataKey key, BotStoreType botStoreType, BotData botData, CancellationToken cancellationToken)
        {
            lock (locks[botStoreType])
            {
                store.AddOrUpdate(GetKey(key, botStoreType), JsonConvert.SerializeObject(botData), (dictionaryKey, value) =>
                {
                    if (botData.ETag != "*" && JsonConvert.DeserializeObject<BotData>(value).ETag != botData.ETag)
                    {
                        throw new HttpException((int)HttpStatusCode.PreconditionFailed, "Inconsistent SaveAsync based on Etag!");
                    }
                    botData.ETag = DateTime.UtcNow.ToString();
                    return Serialize(botData);
                });
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(BotDataKey key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }

        private string GetKey(BotDataKey key, BotStoreType botStoreType)
        {
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    return $"conversation:{key.BotId}:{key.ChannelId}:{key.ConversationId}";
                case BotStoreType.BotUserData:
                    return $"user:{key.BotId}:{key.ChannelId}:{key.UserId}";
                case BotStoreType.BotPrivateConversationData:
                    return $"privateConversation:{key.BotId}:{key.ChannelId}:{key.UserId}:{key.ConversationId}";
                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }

        private static string Serialize(BotData data)
        {
            using (var cmpStream = new MemoryStream())
            using (var stream = new GZipStream(cmpStream, CompressionMode.Compress))
            using (var streamWriter = new StreamWriter(stream))
            {
                var serializedJSon = JsonConvert.SerializeObject(data);
                streamWriter.Write(serializedJSon);
                streamWriter.Close();
                stream.Close();
                return Convert.ToBase64String(cmpStream.ToArray());
            }
        }

        private static BotData Deserialize(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);
            using (var stream = new MemoryStream(bytes))
            using (var gz = new GZipStream(stream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(gz))
            {
                return JsonConvert.DeserializeObject<BotData>(streamReader.ReadToEnd());
            }
        }
    }

    public class ConnectorStore : IBotDataStore<BotData>
    {
        private readonly IStateClient stateClient; 
        public ConnectorStore(IStateClient stateClient)
        {
            SetField.NotNull(out this.stateClient, nameof(stateClient), stateClient);
        }

        async Task<BotData> IBotDataStore<BotData>.LoadAsync(BotDataKey key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            BotData botData;
            switch(botStoreType)
            {
                case BotStoreType.BotConversationData:
                    botData = await stateClient.BotState.GetConversationDataAsync(key.ChannelId, key.ConversationId, cancellationToken);
                    break;
                case BotStoreType.BotUserData:
                    botData = await stateClient.BotState.GetUserDataAsync(key.ChannelId, key.UserId, cancellationToken);
                    break;
                case BotStoreType.BotPrivateConversationData:
                    botData = await stateClient.BotState.GetPrivateConversationDataAsync(key.ChannelId, key.ConversationId, key.UserId, cancellationToken);
                    break;
                default:
                    throw new ArgumentException($"{botStoreType} is not a valid store type!");
            }
            return botData; 
        }

        async Task IBotDataStore<BotData>.SaveAsync(BotDataKey key, BotStoreType botStoreType, BotData botData, CancellationToken cancellationToken)
        {
            switch(botStoreType)
            {
                case BotStoreType.BotConversationData:
                    await stateClient.BotState.SetConversationDataAsync(key.ChannelId, key.ConversationId, botData, cancellationToken);
                    break;
                case BotStoreType.BotUserData:
                    await stateClient.BotState.SetUserDataAsync(key.ChannelId, key.UserId, botData, cancellationToken);
                    break;
                case BotStoreType.BotPrivateConversationData:
                    await stateClient.BotState.SetPrivateConversationDataAsync(key.ChannelId, key.ConversationId, key.UserId, botData, cancellationToken);
                    break;
                default:
                    throw new ArgumentException($"{botStoreType} is not a valid store type!");
            }
        }

        Task<bool> IBotDataStore<BotData>.FlushAsync(BotDataKey key, CancellationToken cancellationToken)
        {
            // Everything is saved. Flush is no-op
            return Task.FromResult(true);
        }
    }

    /// <summary>
    /// Caches data for <see cref="BotDataBase{T}"/> and wraps the data in <see cref="BotData"/> to be stored in <see cref="CachingBotDataStore_LastWriteWins.inner"/>
    /// </summary>
    /// <remarks> 
    /// It sets <see cref="BotData.ETag"/> to "*" when it flushes the data to storage. 
    /// As a result last write will overwrite the data.
    /// </remarks>
    public class CachingBotDataStore_LastWriteWins : IBotDataStore
    {
        private readonly IBotDataStore<BotData> inner;
        internal readonly Dictionary<BotDataKey, DataEntry> cache = new Dictionary<BotDataKey, DataEntry>(); 

        public CachingBotDataStore_LastWriteWins(IBotDataStore<BotData> inner)
        {
            SetField.NotNull(out this.inner, nameof(inner), inner);
        }

        internal class DataEntry
        {
            public object BotConversationData { set; get; }
            public object BotPrivateConversationData { set; get; }
            public object BotUserData { set; get;} 
        }

        async Task<bool> IBotDataStore<object>.FlushAsync(BotDataKey key, CancellationToken cancellationToken)
        {
            DataEntry entry = default(DataEntry);
            if (cache.TryGetValue(key, out entry))
            {
                cache.Remove(key);
                await this.Save(key, entry, cancellationToken);
                return true; 
            }
            else
            {
                return false;
            }
        }

        async Task<object> IBotDataStore<object>.LoadAsync(BotDataKey key, BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            DataEntry entry;
            object obj = null;
            if (!cache.TryGetValue(key, out entry))
            {
                entry = new DataEntry();
                cache[key] = entry;

                BotData value = await inner.LoadAsync(key, botStoreType, cancellationToken);

                if (value?.Data != null)
                {
                    obj = value.Data;
                }

                SetValue(entry, botStoreType, obj);
                return obj;
            }
            else
            {
                switch (botStoreType)
                {
                    case BotStoreType.BotConversationData:
                        if (entry.BotConversationData != null)
                        {
                            obj = entry.BotConversationData;
                        }
                        break;
                    case BotStoreType.BotPrivateConversationData:
                        if (entry.BotPrivateConversationData != null)
                        {
                            obj = entry.BotPrivateConversationData;
                        }
                        break;
                    case BotStoreType.BotUserData:
                        if (entry.BotUserData != null)
                        {
                            obj = entry.BotUserData;
                        }
                        break;
                }

                if(obj == null)
                {
                    BotData value = await inner.LoadAsync(key, botStoreType, cancellationToken);

                    if (value?.Data != null)
                    {
                        obj = value.Data;
                        SetValue(entry, botStoreType, obj);
                    }
                }

                return obj;
            }
        }

        async Task IBotDataStore<object>.SaveAsync(BotDataKey key, BotStoreType botStoreType, object value, CancellationToken cancellationToken)
        {
            DataEntry entry;
            if (!cache.TryGetValue(key, out entry))
            {
                entry = new DataEntry();
                cache[key] = entry;
            }

            SetValue(entry, botStoreType, value);
        }

        private void SetValue(DataEntry entry, BotStoreType botStoreType, object value)
        {
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    entry.BotConversationData = value;
                    break;
                case BotStoreType.BotPrivateConversationData:
                    entry.BotPrivateConversationData = value;
                    break;
                case BotStoreType.BotUserData:
                    entry.BotUserData = value;
                    break;
            }
        }

        private string GetKey(BotDataKey key, BotStoreType botStoreType)
        {
            switch (botStoreType)
            {
                case BotStoreType.BotConversationData:
                    return $"conversation:{key.BotId}:{key.ConversationId}";
                case BotStoreType.BotUserData:
                    return $"user:{key.BotId}:{key.UserId}";
                case BotStoreType.BotPrivateConversationData:
                    return $"privateConversation:{key.BotId}:{key.UserId}:{key.ConversationId}";
                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }

        private async Task Save(BotDataKey key, DataEntry entry, CancellationToken cancellationToken)
        {
            if(entry?.BotConversationData != null)
            {
                await inner.SaveAsync(key, BotStoreType.BotConversationData, new BotData { ETag = "*", Data = entry.BotConversationData }, cancellationToken);
            }

            if(entry?.BotUserData != null)
            {
                await inner.SaveAsync(key, BotStoreType.BotUserData, new BotData { ETag = "*", Data = entry.BotUserData }, cancellationToken);
            }

            if(entry?.BotPrivateConversationData != null)
            {
                await inner.SaveAsync(key, BotStoreType.BotPrivateConversationData, new BotData { ETag = "*", Data = entry.BotPrivateConversationData }, cancellationToken);
            }
        }
    }

    public abstract class BotDataBase<T> : IBotData
    {
        protected readonly IBotDataStore botDataStore;
        protected readonly BotDataKey botDataKey;
        private IBotDataBag conversationData;
        private IBotDataBag privateConversationData;
        private IBotDataBag userData;

        public BotDataBase(IBotIdResolver botIdResolver, IMessageActivity message, IBotDataStore botDataStore)
        {
            SetField.NotNull(out this.botDataStore, nameof(BotDataBase<T>.botDataStore), botDataStore);
            SetField.CheckNull(nameof(message), message);
            this.botDataKey = message.GetBotDataKey(botIdResolver.BotId); 
        }

        protected abstract T MakeData();
        protected abstract IBotDataBag WrapData(T data);

        public async Task LoadAsync(CancellationToken cancellationToken)
        {
            var conversationTask = LoadData(BotStoreType.BotConversationData, cancellationToken);
            var privateConversationTask = LoadData(BotStoreType.BotPrivateConversationData, cancellationToken);
            var userTask = LoadData(BotStoreType.BotUserData, cancellationToken);

            this.conversationData = await conversationTask;
            this.privateConversationData = await privateConversationTask;
            this.userData = await userTask; 
        }

        public async Task FlushAsync(CancellationToken cancellationToken)
        {
            await this.botDataStore.FlushAsync(botDataKey, cancellationToken);
        }

        IBotDataBag IBotData.ConversationData
        {
            get
            {
                CheckNull(nameof(conversationData), conversationData);
                return this.conversationData;
            }
        }

        IBotDataBag IBotData.PrivateConversationData
        {
            get
            {
                CheckNull(nameof(privateConversationData), privateConversationData);
                return this.privateConversationData;
            }
        }

        IBotDataBag IBotData.UserData
        {
            get
            {
                CheckNull(nameof(userData), userData);
                return this.userData;
            }
        }

        private async Task<IBotDataBag> LoadData(BotStoreType botStoreType, CancellationToken cancellationToken)
        {
            T data = (T)await this.botDataStore.LoadAsync(botDataKey, botStoreType, cancellationToken);
            if (data == null)
            {
                data = this.MakeData();
                await this.botDataStore.SaveAsync(botDataKey, botStoreType, data, cancellationToken);
            }
            return this.WrapData(data);
        }

        private void CheckNull(string name, IBotDataBag value)
        {
            if (value == null)
            {
                throw new InvalidOperationException($"{name} cannot be null! probably forgot to call LoadAsync() first!");
            }
        }
    }

    public sealed class DictionaryBotData : BotDataBase<Dictionary<string, object>>
    {
        public DictionaryBotData(IBotIdResolver botIdResolver, IMessageActivity message, IBotDataStore botDataStore)
            : base(botIdResolver, message, botDataStore)
        {
        }

        protected override Dictionary<string, object> MakeData()
        {
            return new Dictionary<string, object>();
        }

        private sealed class Bag : IBotDataBag
        {
            private readonly Dictionary<string, object> bag;
            public Bag(Dictionary<string, object> bag)
            {
                SetField.NotNull(out this.bag, nameof(bag), bag);
            }

            int IBotDataBag.Count { get { return this.bag.Count; } }

            void IBotDataBag.SetValue<T>(string key, T value)
            {
                this.bag[key] = value;
            }

            bool IBotDataBag.TryGetValue<T>(string key, out T value)
            {
                object boxed;
                bool found = this.bag.TryGetValue(key, out boxed);
                if (found)
                {
                    if (boxed is T)
                    {
                        value = (T)boxed;
                        return true;
                    }
                }

                value = default(T);
                return false;
            }

            bool IBotDataBag.RemoveValue(string key)
            {
                return this.bag.Remove(key);
            }

            void IBotDataBag.Clear()
            {
                this.bag.Clear();
            }
        }

        protected override IBotDataBag WrapData(Dictionary<string, object> data)
        {
            return new Bag(data);
        }
    }

    public sealed class JObjectBotData : BotDataBase<JObject>
    {
        public JObjectBotData(IBotIdResolver botIdResolver, IMessageActivity message, IBotDataStore botDataStore)
            : base(botIdResolver, message, botDataStore)
        {
        }

        protected override JObject MakeData()
        {
            return new JObject();
        }
        private sealed class Bag : IBotDataBag
        {
            private readonly JObject bag;
            public Bag(JObject bag)
            {
                SetField.NotNull(out this.bag, nameof(bag), bag);
            }

            int IBotDataBag.Count { get { return this.bag.Count; } }

            void IBotDataBag.SetValue<T>(string key, T value)
            {
                var token = JToken.FromObject(value);
#if DEBUG
                var copy = token.ToObject<T>();
#endif
                this.bag[key] = token;
            }

            bool IBotDataBag.TryGetValue<T>(string key, out T value)
            {
                JToken token;
                bool found = this.bag.TryGetValue(key, out token);
                if (found)
                {
                    value = token.ToObject<T>();
                    return true;
                }

                value = default(T);
                return false;
            }

            bool IBotDataBag.RemoveValue(string key)
            {
                return this.bag.Remove(key);
            }

            void IBotDataBag.Clear()
            {
                this.bag.RemoveAll();
            }

        }

        protected override IBotDataBag WrapData(JObject data)
        {
            return new Bag(data);
        }
    }

    public sealed class BotDataBagStream : MemoryStream
    {
        private readonly IBotDataBag bag;
        private readonly string key;
        public BotDataBagStream(IBotDataBag bag, string key)
        {
            SetField.NotNull(out this.bag, nameof(bag), bag);
            SetField.NotNull(out this.key, nameof(key), key);

            byte[] blob;
            if (this.bag.TryGetValue(key, out blob))
            {
                this.Write(blob, 0, blob.Length);
                this.Position = 0;
            }
        }

        public override void Flush()
        {
            base.Flush();

            var blob = this.ToArray();
            this.bag.SetValue(this.key, blob);
        }

        public override void Close()
        {
            this.Flush();
            base.Close();
        }
    }

    public interface IBotIdResolver
    {
        string BotId { get; }
    }

    public sealed class BotIdResolver : IBotIdResolver
    {
        private readonly string botId;

        public string BotId { get { return botId; } }

        public BotIdResolver(string botId = null)
        {
           SetField.NotNull(out this.botId, nameof(botId), botId ?? ConfigurationManager.AppSettings["BotId"] ?? ConfigurationManager.AppSettings["MicrosoftAppId"]);
        }
    }

    public static partial class Extensions
    {
        public static BotDataKey GetBotDataKey(this IMessageActivity message, string botId)
        {
            SetField.CheckNull(nameof(botId), botId);
            return new BotDataKey
            {
                BotId =  botId,
                UserId = message.From.Id,
                ConversationId = message.Conversation.Id,
                ChannelId = message.ChannelId
            };
        }
    }
}
