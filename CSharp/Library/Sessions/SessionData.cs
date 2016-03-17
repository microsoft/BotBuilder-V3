using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    internal sealed class InMemorySessionData : ISessionData
    {
        private readonly Dictionary<string, object> userData = new Dictionary<string, object>();
        private readonly Dictionary<string, object> conversationData = new Dictionary<string, object>();
        private readonly Dictionary<string, object> perUserInConversationData = new Dictionary<string, object>();

        void ISessionData.Clear()
        {
            this.userData.Clear();
            this.conversationData.Clear();
            this.perUserInConversationData.Clear();
        }

        ReadOnlyDictionary<string, object> ISessionData.ConversationData
        {
            get
            {
                return new ReadOnlyDictionary<string, object>(conversationData);
            }
        }

        ReadOnlyDictionary<string, object> ISessionData.PerUserInConversationData
        {
            get
            {
                return new ReadOnlyDictionary<string, object>(perUserInConversationData);
            }
        }

        ReadOnlyDictionary<string, object> ISessionData.UserData
        {
            get
            {
                return new ReadOnlyDictionary<string, object>(userData);
            }
        }

        void ISessionData.SetConversationData(string key, object value)
        {
            SetData(conversationData, key, value);
        }

        object ISessionData.GetConversationData(string key)
        {
            return GetData(conversationData, key);
        }

        void ISessionData.SetPerUserInConversationData(string key, object value)
        {
            SetData(perUserInConversationData, key, value);
        }

        object ISessionData.GetPerUserInConversationData(string key)
        {
            return GetData(perUserInConversationData, key);
        }

        void ISessionData.SetUserData(string key, object value)
        {
            SetData(userData, key, value);
        }

        object ISessionData.GetUserData(string key)
        {
            return GetData(userData, key);
        }

        private static object GetData(Dictionary<string, object> store, string key)
        {
            object value;
            if (!store.TryGetValue(key, out value))
            {
                value = null;
            }

            return value;
        }

        private static void SetData(Dictionary<string, object> store, string key, object value)
        {
            if (value == null)
            {
                bool removed = store.Remove(key);
            }
            else
            {
                store[key] = value;
            }
        }
    }
}
