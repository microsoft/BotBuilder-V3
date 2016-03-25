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
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Fibers
{
    public static class Methods
    {
        public static Rest<T> Identity<T>()
        {
            return IdentityMethod<T>.Instance.IdentityAsync;
        }

        public static Rest<T> Loop<T>(Rest<T> rest, int count)
        {
            var loop = new LoopMethod<T>(rest, count);
            return loop.LoopAsync;
        }

        public static Rest<T> Void<T>(Rest<T> rest)
        {
            var root = new VoidMethod<T>(rest);
            return root.RootAsync;
        }

        [Serializable]
        private sealed class IdentityMethod<T>
        {
            public static readonly IdentityMethod<T> Instance = new IdentityMethod<T>();

            private IdentityMethod()
            {
            }

            public async Task<IWait> IdentityAsync(IFiber fiber, IItem<T> item)
            {
                return fiber.Done(await item);
            }
        }

        [Serializable]
        private sealed class LoopMethod<T>
        {
            private readonly Rest<T> rest;
            private int count;
            private T item;

            public LoopMethod(Rest<T> rest, int count)
            {
                Field.SetNotNull(out this.rest, nameof(rest), rest);
                this.count = count;
            }

            public async Task<IWait> LoopAsync(IFiber fiber, IItem<T> item)
            {
                this.item = await item;

                --this.count;
                if (this.count >= 0)
                {
                    return fiber.Call<T, object>(this.rest, this.item, NextAsync);
                }
                else
                {
                    return fiber.Done(this.item);
                }
            }

            public async Task<IWait> NextAsync(IFiber fiber, IItem<object> ignore)
            {
                --this.count;
                if (this.count >= 0)
                {
                    return fiber.Call<T, object>(this.rest, this.item, NextAsync);
                }
                else
                {
                    return fiber.Done(this.item);
                }
            }
        }

        [Serializable]
        private sealed class VoidMethod<T>
        {
            private readonly Rest<T> rest;

            public VoidMethod(Rest<T> rest)
            {
                Field.SetNotNull(out this.rest, nameof(rest), rest);
            }

            public async Task<IWait> RootAsync(IFiber fiber, IItem<T> item)
            {
                return fiber.Call<T, object>(this.rest, await item, DoneAsync);
            }

            public async Task<IWait> DoneAsync(IFiber fiber, IItem<object> ignore)
            {
                return NullWait.Instance;
            }
        }
    }
}
