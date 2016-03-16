using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public static partial class Field
    {
        public static void SetNotNull<T>(ref T field, string name, T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            field = value;
        }
    }

    public static partial class Extensions
    {
        public static V GetOrAdd<K, V>(this Dictionary<K, V> valueByKey, K key) where V : new()
        {
            V value;
            if (!valueByKey.TryGetValue(key, out value))
            {
                value = new V();
                valueByKey.Add(key, value);
            }

            return value;
        }
    }
}
