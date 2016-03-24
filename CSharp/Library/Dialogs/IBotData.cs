using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public interface IBotDataBag
    {
        int Count { get; }
        bool TryGetValue<T>(string key, out T value);
        void SetValue<T>(string key, T value);
    }

    public interface IBotData
    {
        IBotDataBag UserData { get; }

        IBotDataBag ConversationData { get; }

        IBotDataBag PerUserInConversationData { get; }
    }

    public static partial class Extensions
    {
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
