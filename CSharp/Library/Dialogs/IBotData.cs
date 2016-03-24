using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A property bag of bot data.
    /// </summary>
    public interface IBotDataBag
    {
        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="IBotDataBag"/>.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to set.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the value associated with the specified key, if the key is found;
        /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>true if the <see cref="IBotDataBag"/> contains an element with the specified key; otherwise, false.</returns>
        bool TryGetValue<T>(string key, out T value);

        /// <summary>
        /// Adds the specified key and value to the bot data bag.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be null for reference types.</param>
        void SetValue<T>(string key, T value);
    }

    /// <summary>
    /// Private bot data.
    /// </summary>
    public interface IBotData
    {
        /// <summary>
        /// Private bot data associated with a user (across all channels and conversations).
        /// </summary>
        IBotDataBag UserData { get; }

        /// <summary>
        /// Private bot data associated with a conversation.
        /// </summary>
        IBotDataBag ConversationData { get; }

        /// <summary>
        /// Private bot data associated with a user in a conversation.
        /// </summary>
        IBotDataBag PerUserInConversationData { get; }
    }

    /// <summary>
    /// Helper methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="bag">The bot data bag.</param>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, a get operation throws a KeyNotFoundException.</returns>
        public static T Get<T>(this IBotDataBag bag, string key)
        {
            T value;
            if (!bag.TryGetValue(key, out value))
            {
                throw new KeyNotFoundException(key);
            }

            return value;
        }
    }
}
