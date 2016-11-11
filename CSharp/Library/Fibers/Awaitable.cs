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
using System.Runtime.CompilerServices;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    public interface IAwaiter<out T> : INotifyCompletion
    {
        bool IsCompleted { get; }

        T GetResult();
    }

    public sealed class AwaiterFromItem<T> : IAwaiter<T>
    {
        private readonly T item;

        public AwaiterFromItem(T item)
        {
            this.item = item;
        }
        
        public bool IsCompleted
        {
            get { return true; }
        }

        public T GetResult()
        {
            return item;
        }

        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }
    }
}

namespace Microsoft.Bot.Builder.Dialogs
{
    using Microsoft.Bot.Builder.Internals.Fibers;

    /// <summary>
    /// Explicit interface to support the compiling of async/await.
    /// </summary>
    /// <typeparam name="T">The type of the contained value.</typeparam>
    public interface IAwaitable<out T>
    {
        /// <summary>
        /// Get the awaiter for this awaitable item.
        /// </summary>
        /// <returns>The awaiter.</returns>
        Builder.Internals.Fibers.IAwaiter<T> GetAwaiter();
    }

    /// <summary>
    /// Creates a <see cref="IAwaitable{T}"/> from item passed to constructor
    /// </summary>
    /// <typeparam name="T"> The type of the item.</typeparam>
    public sealed class AwaitableFromItem<T> : IAwaitable<T>
    {
        private readonly IAwaiter<T> awaiter;

        public AwaitableFromItem(T item)
        {
            this.awaiter = new AwaiterFromItem<T>(item);
        }

        public IAwaiter<T> GetAwaiter()
        {
            return this.awaiter;
        }
    }

    public static partial class Extensions
    {
        /// <summary>
        /// Wraps item in a <see cref="IAwaitable{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of the item.</typeparam>
        /// <param name="item">The item that will be wrapped.</param>
        public static IAwaitable<T> GetAwaitable<T>(this T item)
        {
            return  new AwaitableFromItem<T>(item);
        }
    }
}
