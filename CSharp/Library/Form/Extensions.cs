using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.Form.Advanced
{
    internal static class Extensions
    {
        internal static bool IsICollection(this Type type)
        {
            return Array.Exists(type.GetInterfaces(), IsGenericCollectionType);
        }

        internal static bool IsIEnumerable(this Type type)
        {
            return Array.Exists(type.GetInterfaces(), IsGenericEnumerableType);
        }

        internal static bool IsIList(this Type type)
        {
            return Array.Exists(type.GetInterfaces(), IsListCollectionType);
        }

        internal static bool IsGenericCollectionType(this Type type)
        {
            return type.IsGenericType && (typeof(ICollection<>) == type.GetGenericTypeDefinition());
        }

        internal static bool IsGenericEnumerableType(this Type type)
        {
            return type.IsGenericType && (typeof(IEnumerable<>) == type.GetGenericTypeDefinition());
        }

        internal static bool IsIntegral(this Type type)
        {
            return (type == typeof(sbyte) ||
                    type == typeof(byte) ||
                    type == typeof(short) ||
                    type == typeof(ushort) ||
                    type == typeof(int) ||
                    type == typeof(uint) ||
                    type == typeof(long) ||
                    type == typeof(ulong));
        }

        internal static bool IsDouble(this Type type)
        {
            return type == typeof(float) || type == typeof(double);
        }

        internal static bool IsListCollectionType(this Type type)
        {
            return type.IsGenericType && (typeof(IList<>) == type.GetGenericTypeDefinition());
        }

        internal static bool IsNullable(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        internal static Type GetGenericElementType(this Type type)
        {
            return (from i in type.GetInterfaces()
                    where i.IsGenericType && typeof(IEnumerable<>) == i.GetGenericTypeDefinition()
                    select i.GetGenericArguments()[0]).FirstOrDefault();
        }
    }
}
