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
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    public enum BotStoreType
    {
        BotConversationData,
        BotPerUserInConversationData,
        BotUserData
    }

    public class BotDataKey
    {
        public string UserId { get; set; }
        public string ConversationId { get; set; }
        public string BotId { get; set; }

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
                    otherKey.BotId == this.BotId;
            }
        }

        public override int GetHashCode()
        {
            return UserId.GetHashCode() + 
                ConversationId.GetHashCode() + 
                BotId.GetHashCode();
        }
    }

    public interface IBotDataStore
    {
        Task<T> LoadAsync<T>(BotDataKey key, BotStoreType botStoreType);

        Task SaveAsync(BotDataKey key, BotStoreType botStoreType, object value);

        Task<bool> FlushAsync(BotDataKey key); 
    }
    
    public class MessageBackedStore : IBotDataStore
    {
        protected readonly Message message;
        public MessageBackedStore(Message message)
        {
            SetField.NotNull(out this.message, nameof(message), message);
        }

        private bool ValidateKey(BotDataKey key)
        {
            return key.UserId == message.From?.Id &&
                   key.BotId == message.To?.Id &&
                   key.ConversationId == message.ConversationId;
        }

        public Task<bool> FlushAsync(BotDataKey key)
        {
            // no-op for message backed store
            return Task.FromResult(ValidateKey(key));
        }

        public Task SaveAsync(BotDataKey key, BotStoreType botStoreType, object value)
        {
            try
            {
                if (!ValidateKey(key))
                {
                    throw new ArgumentException("Invalid bot data key!");
                }

                switch (botStoreType)
                {
                    case BotStoreType.BotConversationData:
                        message.BotConversationData = value;
                        break;
                    case BotStoreType.BotPerUserInConversationData:
                        message.BotPerUserInConversationData = value;
                        break;
                    case BotStoreType.BotUserData:
                        message.BotUserData = value;
                        break;
                    default:
                        throw new ArgumentException($"key {botStoreType} is not supported by this message store");
                }

                return Task.FromResult(Type.Missing);
            }
            catch(Exception e)
            {
                return Task.FromException(e);
            }
        }

        public Task<T> LoadAsync<T>(BotDataKey key, BotStoreType botStoreType)
        {
            try
            {
                if (!ValidateKey(key))
                {
                    throw new ArgumentException("Invalid bot data key!");
                }

                var value = default(T);

                switch (botStoreType.ToString())
                {
                    case nameof(message.BotConversationData):
                        value = (T)message.BotConversationData;
                        break;
                    case nameof(message.BotPerUserInConversationData):
                        value = (T)message.BotPerUserInConversationData;
                        break;
                    case nameof(message.BotUserData):
                        value = (T)message.BotUserData;
                        break;
                    default:
                        value = default(T);
                        break;
                }
                return Task.FromResult(value);
            }
            catch (Exception e)
            {
                return Task.FromException<T>(e);
            }
        }
    }

    public class InMemoryBotDataStore : IBotDataStore
    {
        //TODO: ensure data consistency between store and cache.
        internal readonly ConcurrentDictionary<string, string> store = new ConcurrentDictionary<string, string>();

        internal readonly ConcurrentDictionary<BotDataKey, DataEntry> cache = new ConcurrentDictionary<BotDataKey, DataEntry>(); 

        internal class DataEntry
        {
            public object BotConversationData { set; get; }
            public object BotPerUserInConversationData { set; get; }
            public object BotUserData { set; get;} 
        }

        public async Task<bool> FlushAsync(BotDataKey key)
        {
            DataEntry entry = default(DataEntry);
            if (cache.TryRemove(key, out entry))
            {
                this.Save(key, entry);
                return true; 
            }
            else
            {
                return false;
            }
        }

        public async Task<T> LoadAsync<T>(BotDataKey key, BotStoreType botStoreType)
        {
            DataEntry entry;
            T obj = default(T);
            if (!cache.TryGetValue(key, out entry))
            {
                entry = new DataEntry();
                cache[key] = entry;

                string value;

                if (store.TryGetValue(GetKey(key, botStoreType), out value))
                {
                    obj = (T)JsonConvert.DeserializeObject(value);
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
                            obj = (T)entry.BotConversationData;
                        }
                        break;
                    case BotStoreType.BotPerUserInConversationData:
                        if (entry.BotPerUserInConversationData != null)
                        {
                            obj = (T)entry.BotPerUserInConversationData;
                        }
                        break;
                    case BotStoreType.BotUserData:
                        if (entry.BotUserData != null)
                        {
                            obj = (T)entry.BotUserData;
                        }
                        break;
                }

                if(obj == null)
                {
                    string value;
                    if (store.TryGetValue(GetKey(key, botStoreType), out value))
                    {
                        obj = (T)JsonConvert.DeserializeObject(value);
                        SetValue(entry, botStoreType, obj);
                    }
                }

                return obj;
            }
        }

        public async Task SaveAsync(BotDataKey key, BotStoreType botStoreType, object value)
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
                case BotStoreType.BotPerUserInConversationData:
                    entry.BotPerUserInConversationData = value;
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
                case BotStoreType.BotPerUserInConversationData:
                    return $"perUserInConversation:{key.BotId}:{key.UserId}:{key.ConversationId}";
                default:
                    throw new ArgumentException("Unsupported bot store type!");
            }
        }

        private void Save(BotDataKey key, DataEntry entry)
        {
            if(entry?.BotConversationData != null)
            {
                store[GetKey(key, BotStoreType.BotConversationData)] = JsonConvert.SerializeObject(entry.BotConversationData);
            }

            if(entry?.BotUserData != null)
            {
                store[GetKey(key, BotStoreType.BotUserData)] = JsonConvert.SerializeObject(entry.BotUserData);
            }

            if(entry?.BotPerUserInConversationData != null)
            {
                store[GetKey(key, BotStoreType.BotPerUserInConversationData)] = JsonConvert.SerializeObject(entry.BotPerUserInConversationData);
            }
        }

    }

    public abstract class BotDataBase<T> : IBotData
    {
        protected readonly IBotDataStore botDataStore;
        protected readonly BotDataKey botDataKey;
        private IBotDataBag conversationData;
        private IBotDataBag perUserInConversationData;
        private IBotDataBag userData;

        public BotDataBase(Message message, IBotDataStore botDataStore)
        {
            SetField.NotNull(out this.botDataStore, nameof(BotDataBase<T>.botDataStore), botDataStore);
            SetField.CheckNull(nameof(message), message);
            this.botDataKey = message.GetBotDataKey(); 
        }

        protected abstract T MakeData();
        protected abstract IBotDataBag WrapData(T data);

        public async Task LoadAsync()
        {
            var conversationTask = LoadData(BotStoreType.BotConversationData);
            var perUserInConversationTask = LoadData(BotStoreType.BotPerUserInConversationData);
            var userTask = LoadData(BotStoreType.BotUserData);

            this.conversationData = await conversationTask;
            this.perUserInConversationData = await perUserInConversationTask;
            this.userData = await userTask; 
        }

        public async Task FlushAsync()
        {
            await this.botDataStore.FlushAsync(botDataKey);
        }

        IBotDataBag IBotData.ConversationData
        {
            get
            {
                CheckNull(nameof(conversationData), conversationData);
                return this.conversationData;
            }
        }

        IBotDataBag IBotData.PerUserInConversationData
        {
            get
            {
                CheckNull(nameof(perUserInConversationData), perUserInConversationData);
                return this.perUserInConversationData;
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

        private async Task<IBotDataBag> LoadData(BotStoreType botStoreType)
        {
            T data = await this.botDataStore.LoadAsync<T>(botDataKey, botStoreType);
            if (data == null)
            {
                data = this.MakeData();
                await this.botDataStore.SaveAsync(botDataKey, botStoreType, data);
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
        public DictionaryBotData(Message message, IBotDataStore botDataStore)
            : base(message, botDataStore)
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
        }

        protected override IBotDataBag WrapData(Dictionary<string, object> data)
        {
            return new Bag(data);
        }
    }

    public sealed class JObjectBotData : BotDataBase<JObject>
    {
        public JObjectBotData(Message message, IBotDataStore botDataStore)
            : base(message, botDataStore)
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

    public static partial class Extensions
    {
        public static BotDataKey GetBotDataKey(this Message message)
        {
            var toIsBot = message.To.IsBot.HasValue && message.To.IsBot.Value;
            return new BotDataKey
            {
                BotId =  toIsBot ? message.To.Id : message.From.Id,
                UserId = toIsBot ? message.From.Id : message.To.Id,
                ConversationId = message.ConversationId
            };
        }
    }
}
