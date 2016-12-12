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
using System.Text;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    public static partial class Extensions
    {
        // TODO: split off R to get better type inference on T
        public static IWait<C> Call<C, T, R>(this IFiber<C> fiber, Rest<C, T> invokeHandler, T item, Rest<C, R> returnHandler)
        {
            // tell the leaf frame of the stack to wait for the return value
            var wait = fiber.Waits.Make<R>();
            wait.Wait(returnHandler);
            fiber.Wait = wait;
            
            // call the child
            return fiber.Call<C, T>(invokeHandler, item);
        }

        public static IWait<C> Call<C, T>(this IFiber<C> fiber, Rest<C, T> invokeHandler, T item)
        {
            // make a frame on the stack for calling the method
            fiber.Push();
            
            // initiate and immediately compete a wait for calling the child
            var wait = fiber.Waits.Make<T>();
            wait.Wait(invokeHandler);
            wait.Post(item);
            fiber.Wait = wait;
            return wait;
        }

        public static IWait<C> Wait<C, T>(this IFiber<C> fiber, Rest<C, T> resumeHandler)
        {
            var wait = fiber.Waits.Make<T>();
            wait.Wait(resumeHandler);
            fiber.Wait = wait;
            return wait;
        }

        public static IWait<C> Done<C, T>(this IFiber<C> fiber, T item)
        {
            // pop the stack
            fiber.Done();

            // complete the caller's wait for the return value
            fiber.Wait.Post(item);
            return fiber.Wait;
        }

        public static void Reset<C>(this IFiber<C> fiber)
        {
            while (fiber.Frames.Count > 0)
            {
                fiber.Done();
            }
        }

        public static IWait<C> Post<C, T>(this IFiber<C> fiber, T item)
        {
            fiber.Wait.Post(item);
            return fiber.Wait;
        }

        public static IWait<C> Fail<C>(this IFiber<C> fiber, Exception error)
        {
            // pop the stack
            fiber.Done();

            // complete the caller's wait with an exception
            fiber.Wait.Fail(error);
            return fiber.Wait;
        }

        public static void ValidateNeed(this IWait wait, Need need)
        {
            if (need != wait.Need)
            {
                throw new InvalidNeedException(wait, need);
            }
        }

        public static IWait<C> CloneTyped<C>(this IWait<C> wait)
        {
            return (IWait<C>)wait.Clone();
        }

        public static Task<T> ToTask<T>(this IAwaitable<T> item)
        {
            var source = new TaskCompletionSource<T>();
            try
            {
                var result = item.GetAwaiter().GetResult();
                source.SetResult(result);
            }
            catch (Exception error)
            {
                source.SetException(error);
            }

            return source.Task;
        }

        public static void Push<T>(this IList<T> stack, T item)
        {
            stack.Add(item);
        }

        public static T Pop<T>(this List<T> stack)
        {
            var top = stack.Peek();
            stack.RemoveAt(stack.Count - 1);
            return top;
        }

        public static T Peek<T>(this IReadOnlyList<T> stack)
        {
            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty");
            }

            return stack[stack.Count - 1];
        }

        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            return (T)info.GetValue(name, typeof(T));
        }

        public static V GetOrAdd<K, V>(this IDictionary<K, V> valueByKey, K key, Func<K, V> make)
        {
            V value;
            if (!valueByKey.TryGetValue(key, out value))
            {
                value = make(key);
                valueByKey.Add(key, value);
            }

            return value;
        }

        public static bool Equals<T>(this IReadOnlyList<T> one, IReadOnlyList<T> two, IEqualityComparer<T> comparer)
        {
            if (object.Equals(one, two))
            {
                return true;
            }

            if (one.Count != two.Count)
            {
                return false;
            }

            for (int index = 0; index < one.Count; ++index)
            {
                if (!comparer.Equals(one[index], two[index]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
