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

namespace Microsoft.Bot.Builder.Fibers
{
    public interface IWaiter
    {
        IWait Wait { get; }
        IWait<T> NextWait<T>();
    }

    public interface IFiber : IWaiter
    {
        void Push();
        void Done();
    }

    public interface IFiberLoop : IFiber
    {
        Task<IWait> PollAsync();
    }

    public interface IFrameLoop
    {
        Task<IWait> PollAsync(IFiber fiber);
    }


    public interface IFrame : IWaiter, IFrameLoop
    {
    }

    [Serializable]
    public sealed class Frame : IFrame
    {
        private readonly IWaitFactory factory;
        private IWait wait;

        public Frame(IWaitFactory factory)
        {
            SetField.SetNotNull(out this.factory, nameof(factory), factory);
            this.wait = NullWait.Instance;
        }

        public override string ToString()
        {
            return this.wait.ToString();
        }

        IWait IWaiter.Wait
        {
            get { return this.wait; }
        }

        IWait<T> IWaiter.NextWait<T>()
        {
            if (this.wait is NullWait)
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

        async Task<IWait> IFrameLoop.PollAsync(IFiber fiber)
        {
            return await this.wait.PollAsync(fiber);
        }
    }

    public interface IFrameFactory
    {
        IFrame Make();
    }

    [Serializable]
    public sealed class FrameFactory : IFrameFactory
    {
        private readonly IWaitFactory factory;

        public FrameFactory(IWaitFactory factory)
        {
            SetField.SetNotNull(out this.factory, nameof(factory), factory);
        }

        IFrame IFrameFactory.Make()
        {
            return new Frame(this.factory);
        }
    }

    [Serializable]
    public sealed class Fiber : IFiber, IFiberLoop
    {
        private readonly Stack<IFrame> stack = new Stack<IFrame>();
        private readonly IFrameFactory factory;

        public Fiber(IFrameFactory factory)
        {
            SetField.SetNotNull(out this.factory, nameof(factory), factory);
        }

        public IFrameFactory Factory
        {
            get
            {
                return this.factory;
            }
        }

        void IFiber.Push()
        {
            this.stack.Push(this.factory.Make());
        }

        void IFiber.Done()
        {
            this.stack.Pop();
        }

        IWait IWaiter.Wait
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
                    return NullWait.Instance;
                }
            }
        }

        IWait<T> IWaiter.NextWait<T>()
        {
            var leaf = this.stack.Peek();
            return leaf.NextWait<T>();
        }

        async Task<IWait> IFiberLoop.PollAsync()
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
                    var next = await leaf.PollAsync(this);
                    var peek = this.stack.Peek();
                    bool fine = object.ReferenceEquals(next, peek.Wait) || next is NullWait;
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

            return NullWait.Instance;
        }
    }
}