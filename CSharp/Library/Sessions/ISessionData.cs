using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface ISessionData
    {
        void Clear();
        void SetUserData(string key, object data);
        object GetUserData(string key);
        void SetConversationData(string key, object data);
        object GetConversationData(string key);
        void SetPerUserInConversationData(string key, object data);
        object GetPerUserInConversationData(string key);
        ReadOnlyDictionary<string, object> UserData { get; }
        ReadOnlyDictionary<string, object> ConversationData { get; }
        ReadOnlyDictionary<string, object> PerUserInConversationData { get; }
    }

    public static partial class Extensions
    {
        public static void CopyTo(this ISessionData source, ISessionData target)
        {
            if (!object.ReferenceEquals(source, target))
            {
                foreach (var kv in source.UserData)
                {
                    target.SetUserData(kv.Key, kv.Value);
                }

                foreach (var kv in source.ConversationData)
                {
                    target.SetConversationData(kv.Key, kv.Value);
                }

                foreach (var kv in source.PerUserInConversationData)
                {
                    target.SetPerUserInConversationData(kv.Key, kv.Value);
                }
            }
        }
    }
}
