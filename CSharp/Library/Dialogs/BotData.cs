using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.Serialization;
using Microsoft.Bot.Builder.Fibers;

namespace Microsoft.Bot.Builder
{
    public abstract class BotDataBase<T> : IBotData, Serialization.ISerializeAsReference
    {
        protected readonly Message mesage;

        public BotDataBase(Message message)
        {
            Field.SetNotNull(out this.mesage, nameof(mesage), message);
        }

        protected abstract T MakeData();
        protected abstract IBotDataBag WrapData(T data);

        IBotDataBag IBotData.ConversationData
        {
            get
            {
                var data = (T)this.mesage.BotConversationData;
                if (data == null)
                {
                    data = this.MakeData();
                    this.mesage.BotConversationData = data;
                }

                return this.WrapData(data);
            }
        }

        IBotDataBag IBotData.PerUserInConversationData
        {
            get
            {
                var data = (T)this.mesage.BotPerUserInConversationData;
                if (data == null)
                {
                    data = this.MakeData();
                    this.mesage.BotPerUserInConversationData = data;
                }

                return this.WrapData(data);
            }
        }

        IBotDataBag IBotData.UserData
        {
            get
            {
                var data = (T)this.mesage.BotUserData;
                if (data == null)
                {
                    data = this.MakeData();
                    this.mesage.BotUserData = data;
                }

                return this.WrapData(data);
            }
        }
    }

    public sealed class DictionaryBotData : BotDataBase<Dictionary<string, object>>
    {
        public DictionaryBotData(Message message)
            : base(message)
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
                Field.SetNotNull(out this.bag, nameof(bag), bag);
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
        }

        protected override IBotDataBag WrapData(Dictionary<string, object> data)
        {
            return new Bag(data);
        }
    }

    public sealed class JObjectBotData : BotDataBase<JObject>
    {
        public JObjectBotData(Message message)
            : base(message)
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
                Field.SetNotNull(out this.bag, nameof(bag), bag);
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
        }

        protected override IBotDataBag WrapData(JObject data)
        {
            return new Bag(data);
        }
    }
}
