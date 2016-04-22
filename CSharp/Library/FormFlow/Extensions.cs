// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK Github:
// https://github.com/Microsoft/BotBuilder
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Bot.Builder.FormFlow.Advanced
{
    public static partial class Extensions
    {
        internal static IStep<T> Step<T>(this IForm<T> form, string name) where T : class
        {
            IStep<T> result = null;
            foreach (var step in form.Steps)
            {
                if (step.Name == name)
                {
                    result = step;
                    break;
                }
            }
            return result;
        }

        internal static int StepIndex<T>(this IForm<T> form, IStep<T> step) where T : class
        {
            var index = -1;
            for (var i = 0; i < form.Steps.Count; ++i)
            {
                if (form.Steps[i] == step)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        internal static IField<T> BuildCommandRecognizer<T>(this IForm<T> form) where T : class
        {
            var field = new Field<T>("__commands__", FieldRole.Value);
            field.SetPrompt(new PromptAttribute(""));
            foreach (var entry in form.Configuration.Commands)
            {
                field.AddDescription(entry.Key, entry.Value.Description);
                field.AddTerms(entry.Key, entry.Value.Terms);
            }
            foreach (var nav in form.Fields)
            {
                var fterms = nav.FieldTerms;
                if (fterms != null)
                {
                    field.AddDescription(nav.Name, nav.FieldDescription);
                    field.AddTerms(nav.Name, fterms.ToArray());
                }
            }
            field.Form = form;
            return field;
        }

        internal static IEnumerable<string> Dependencies<T>(this IForm<T> form, int istep)
        {
            for(var i = 0; i < istep; ++i)
            {
                if (form.Steps[i].Type == StepType.Field)
                {
                    yield return form.Steps[i].Name;
                }
            }
        }

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
