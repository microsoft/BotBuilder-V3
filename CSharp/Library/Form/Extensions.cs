using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    public static class Extensions
    {
        public static bool IsICollection(this Type type)
        {
            return Array.Exists(type.GetInterfaces(), IsGenericCollectionType);
        }

        public static bool IsIEnumerable(this Type type)
        {
            return Array.Exists(type.GetInterfaces(), IsGenericEnumerableType);
        }

        public static bool IsIList(this Type type)
        {
            return Array.Exists(type.GetInterfaces(), IsListCollectionType);
        }

        public static bool IsGenericCollectionType(this Type type)
        {
            return type.IsGenericType && (typeof(ICollection<>) == type.GetGenericTypeDefinition());
        }

        public static bool IsGenericEnumerableType(this Type type)
        {
            return type.IsGenericType && (typeof(IEnumerable<>) == type.GetGenericTypeDefinition());
        }

        public static bool IsListCollectionType(this Type type)
        {
            return type.IsGenericType && (typeof(IList<>) == type.GetGenericTypeDefinition());
        }

        public static Type GetGenericElementType(this Type type)
        {
            return (from i in type.GetInterfaces()
                    where i.IsGenericType && typeof(IEnumerable<>) == i.GetGenericTypeDefinition()
                    select i.GetGenericArguments()[0]).FirstOrDefault();
        }
    }
}
