using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Fibers
{
#pragma warning disable CS1998

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
