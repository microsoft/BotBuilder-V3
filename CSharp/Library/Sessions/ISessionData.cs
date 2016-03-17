using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// The data stored in the session, scoped to the user, conversation, or user in conversation.
    /// </summary>
    public interface ISessionData
    {
        /// <summary>
        /// Clear all data.
        /// </summary>
        void Clear();

        /// <summary>
        /// Set data scoped to the user.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <param name="value">Data value.  This value should be serializable.</param>
        void SetUserData(string key, object value);

        /// <summary>
        /// Get data scoped to the user.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <returns>Data value.</returns>
        object GetUserData(string key);

        /// <summary>
        /// Set data scoped to the conversation.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <param name="value">Data value.  This value should be serializable.</param>
        void SetConversationData(string key, object value);

        /// <summary>
        /// Get data scoped to the conversation.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <returns>Data value.</returns>
        object GetConversationData(string key);

        /// <summary>
        /// Set data scoped to the user in the conversation.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <param name="value">Data value.  This value should be serializable.</param>
        void SetPerUserInConversationData(string key, object value);

        /// <summary>
        /// Get data scoped to the user in the conversation.
        /// </summary>
        /// <param name="key">Data key.</param>
        /// <returns>Data value.</returns>
        object GetPerUserInConversationData(string key);

        /// <summary>
        /// Gets the data scoped to the user.
        /// </summary>
        ReadOnlyDictionary<string, object> UserData { get; }

        /// <summary>
        /// Gets the data scoped to the conversation.
        /// </summary>
        ReadOnlyDictionary<string, object> ConversationData { get; }

        /// <summary>
        /// Gets the data scoped to the user in the conversation.
        /// </summary>
        ReadOnlyDictionary<string, object> PerUserInConversationData { get; }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Copy session data.
        /// </summary>
        /// <param name="source">The source session data.</param>
        /// <param name="target">The target session data.</param>
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
