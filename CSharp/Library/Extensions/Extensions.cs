using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public static partial class Field
    {
        public static void SetNotNull<T>(out T field, string name, T value) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }

            field = value;
        }

        public static void SetNotNullFrom<T>(out T field, string name, SerializationInfo info) where T : class
        {
            var value = (T)info.GetValue(name, typeof(T));
            Field.SetNotNull(out field, name, value);
        }

        public static void SetFrom<T>(out T field, string name, SerializationInfo info) where T : struct
        {
            var value = (T)info.GetValue(name, typeof(T));
            field = value;
        }
    }
}
