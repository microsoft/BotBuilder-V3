using System;
using System.Runtime.Serialization;

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

        public static void SetFrom<T>(out T field, string name, SerializationInfo info)
        {
            var value = (T)info.GetValue(name, typeof(T));
            field = value;
        }
    }

    public static partial class Extensions
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }
    }
}
