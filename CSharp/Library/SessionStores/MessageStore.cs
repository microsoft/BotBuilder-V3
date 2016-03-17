using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class MessageStore : ISessionStore
    {
        private Message message; 

        public MessageStore(Message message)
        {
            this.message = message; 
        }

        public void Load(string sessionID, ISessionData sessionData)
        {
            Debug.Assert(message.ConversationId == sessionID);
            var userData = message.BotUserData == null ? null : Serializers.BinarySerializer.GZipDeserialize<Dictionary<string, object>>(message.BotUserData as string);
            var conversationData = message.BotConversationData == null ? null : Serializers.BinarySerializer.GZipDeserialize<Dictionary<string, object>>(message.BotConversationData as string);
            var perUserInConversationData = message.BotPerUserInConversationData == null ? null : Serializers.BinarySerializer.GZipDeserialize<Dictionary<string, object>>(message.BotPerUserInConversationData as string);

            if (userData != null)
            {
                foreach (var data in userData)
                {
                    sessionData.SetUserData(data.Key, data.Value);
                }
            }

            if (conversationData != null)
            {
                foreach (var data in conversationData)
                {
                    sessionData.SetConversationData(data.Key, data.Value);
                }
            }

            if (perUserInConversationData != null)
            {
                foreach (var data in perUserInConversationData)
                {
                    sessionData.SetPerUserInConversationData(data.Key, data.Value);
                }
            }
        }

        public void Save(string sessionID, ISessionData sessionData)
        {
            Debug.Assert(message.ConversationId == sessionID);
            message.BotUserData = Serializers.BinarySerializer.GZipSerialize(sessionData.UserData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            message.BotConversationData = Serializers.BinarySerializer.GZipSerialize(sessionData.ConversationData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
            message.BotPerUserInConversationData = Serializers.BinarySerializer.GZipSerialize(sessionData.PerUserInConversationData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        async Task ISessionStore.LoadAsync(string sessionID, ISessionData sessionData)
        {
            this.Load(sessionID, sessionData);
        }

        async Task ISessionStore.SaveAsync(string sessionID, ISessionData sessionData)
        {
            this.Save(sessionID, sessionData);
        }
    }
}
