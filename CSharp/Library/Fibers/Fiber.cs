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
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    public interface IWaiter<C>
    {
        IWait<C> Wait { get; }
        IWait<C, T> NextWait<T>();
    }

    public interface IFiber<C> : IWaiter<C>
    {
        IReadOnlyList<IFrame<C>> Frames { get; }
        void Push();
        void Done();
    }

    public interface IFiberLoop<C> : IFiber<C>
    {
        Task<IWait<C>> PollAsync(C context);
    }

    public interface IFrameLoop<C>
    {
        Task<IWait<C>> PollAsync(IFiber<C> fiber, C context);
    }


    public interface IFrame<C> : IWaiter<C>, IFrameLoop<C>
    {
    }

    [Serializable]
    public sealed class Frame<C> : IFrame<C>
    {
        private readonly IWaitFactory<C> factory;
        private IWait<C> wait;

        public Frame(IWaitFactory<C> factory)
        {
            SetField.NotNull(out this.factory, nameof(factory), factory);
            this.wait = NullWait<C>.Instance;
        }

        public override string ToString()
        {
            return this.wait.ToString();
        }

        IWait<C> IWaiter<C>.Wait
        {
            get { return this.wait; }
        }

        IWait<C, T> IWaiter<C>.NextWait<T>()
        {
            if (this.wait is NullWait<C>)
            {
                this.wait = null;
            }

            if (this.wait != null)
            {
                if (this.wait.Need != Need.Call)
                {
                    throw new InvalidNeedException(this.wait, Need.Call);
                }

                this.wait = null;
            }

            var wait = this.factory.Make<T>();
            this.wait = wait;
            return wait;
        }

        async Task<IWait<C>> IFrameLoop<C>.PollAsync(IFiber<C> fiber, C context)
        {
            return await this.wait.PollAsync(fiber, context);
        }
    }

    public interface IFrameFactory<C>
    {
        IFrame<C> Make();
    }

    [Serializable]
    public sealed class FrameFactory<C> : IFrameFactory<C>
    {
        private readonly IWaitFactory<C> factory;

        public FrameFactory(IWaitFactory<C> factory)
        {
            SetField.NotNull(out this.factory, nameof(factory), factory);
        }

        IFrame<C> IFrameFactory<C>.Make()
        {
            return new Frame<C>(this.factory);
        }
    }

    [Serializable]
    public sealed class Fiber<C> : IFiber<C>, IFiberLoop<C>
    {
        public delegate IFiberLoop<C> Factory();

        private readonly List<IFrame<C>> stack = new List<IFrame<C>>();
        private readonly IFrameFactory<C> factory;

        public Fiber(IFrameFactory<C> factory)
        {
            SetField.NotNull(out this.factory, nameof(factory), factory);
        }

        public IFrameFactory<C> FrameFactory
        {
            get
            {
                return this.factory;
            }
        }

        IReadOnlyList<IFrame<C>> IFiber<C>.Frames
        {
            get
            {
                return this.stack;
            }
        }

        void IFiber<C>.Push()
        {
            this.stack.Push(this.factory.Make());
        }

        void IFiber<C>.Done()
        {
            this.stack.Pop();
        }

        IWait<C> IWaiter<C>.Wait
        {
            get
            {
                if (this.stack.Count > 0)
                {
                    var leaf = this.stack.Peek();
                    return leaf.Wait;
                }
                else
                {
                    return NullWait<C>.Instance;
                }
            }
        }

        IWait<C, T> IWaiter<C>.NextWait<T>()
        {
            var leaf = this.stack.Peek();
            return leaf.NextWait<T>();
        }

        async Task<IWait<C>> IFiberLoop<C>.PollAsync(C context)
        {
            while (this.stack.Count > 0)
            {
                var leaf = this.stack.Peek();
                var wait = leaf.Wait;
                switch (wait.Need)
                {
                    case Need.None:
                    case Need.Wait:
                    case Need.Done:
                        return wait;
                    case Need.Poll:
                        break;
                    default:
                        throw new InvalidNeedException(wait, Need.Poll);
                }

                try
                {
                    var next = await leaf.PollAsync(this, context);
                    var peek = this.stack.Peek();
                    bool fine = object.ReferenceEquals(next, peek.Wait) || next is NullWait<C>;
                    if (!fine)
                    {
                        throw new InvalidNextException(next);
                    }
                }
                catch (Exception error)
                {
                    this.stack.Pop();
                    if (this.stack.Count == 0)
                    {
                        throw;
                    }
                    else
                    {
                        var parent = this.stack.Peek();
                        parent.Wait.Fail(error);
                    }
                }
            }

            return NullWait<C>.Instance;
        }
    }
}