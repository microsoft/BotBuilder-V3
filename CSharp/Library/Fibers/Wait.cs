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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Fibers
{
    public interface IItem<out T> : IAwaitable<T>
    {
    }

    public delegate Task<IWait> Rest<in T>(IFiber fiber, IItem<T> item);

    public enum Need { None, Wait, Poll, Call, Done };

    public interface IWait
    {
        Need Need { get; }
        Type ItemType { get; }
        Type NeedType { get; }
        Delegate Rest { get; }
        void Post<T>(T item);
        void Fail(Exception error);
        Task<IWait> PollAsync(IFiber fiber);
    }

    public sealed class NullWait : IWait
    {
        public static readonly IWait Instance = new NullWait();
        private NullWait()
        {
        }

        Need IWait.Need
        {
            get
            {
                return Need.None;
            }
        }

        Type IWait.NeedType
        {
            get
            {
                return typeof(object);
            }
        }

        Delegate IWait.Rest
        {
            get
            {
                throw new InvalidNeedException(this, Need.None);
            }
        }

        Type IWait.ItemType
        {
            get
            {
                return typeof(object);
            }
        }

        void IWait.Post<T>(T item)
        {
            throw new InvalidNeedException(this, Need.Wait);
        }

        void IWait.Fail(Exception error)
        {
            throw new InvalidNeedException(this, Need.Wait);
        }

        Task<IWait> IWait.PollAsync(IFiber fiber)
        {
            throw new InvalidNeedException(this, Need.Poll);
        }
    }

    public interface IWait<out T> : IWait
    {
        void Wait(Rest<T> rest);
    }

    public interface IPost<in T>
    {
        void Post(T item);
    }

    public sealed class PostStruct<T> : IPost<T>
    {
        private readonly IPost<object> postBoxed;
        public PostStruct(IPost<object> postBoxed)
        {
            SetField.NotNull(out this.postBoxed, nameof(postBoxed), postBoxed);
        }
        void IPost<T>.Post(T item)
        {
            this.postBoxed.Post((object)item);
        }
    }

    [Serializable]
    public sealed class Wait<T> : IItem<T>, IWait<T>, IPost<T>, IAwaiter<T>, IEquatable<Wait<T>>, ISerializable
    {
        private Rest<T> rest;
        private Need need;
        private T item;
        private Exception fail;

        public Wait()
        {
        }

        private Wait(SerializationInfo info, StreamingContext context)
        {
            SetField.NotNullFrom(out this.rest, nameof(rest), info);
            SetField.From(out this.need, nameof(need), info);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.rest), this.rest);
            info.AddValue(nameof(this.need), this.need);
        }

        public override string ToString()
        {
            IWait wait = this;
            return $"Wait: {wait.Need} {wait.NeedType?.Name} for {this.rest?.Target.GetType().Name}.{this.rest?.Method.Name} have {wait.ItemType?.Name} {this.item}";
        }

        public override int GetHashCode()
        {
            return this.rest.GetHashCode();
        }

        public override bool Equals(object other)
        {
            IEquatable<Wait<T>> wait = this;
            return wait.Equals(other as Wait<T>);
        }

        bool IEquatable<Wait<T>>.Equals(Wait<T> other)
        {
            return other != null
                && object.Equals(this.rest, other.rest)
                && object.Equals(this.need, other.need)
                && object.Equals(this.item, other.item)
                && object.Equals(this.fail, other.fail)
                ;
        }

        Need IWait.Need
        {
            get
            {
                return this.need;
            }
        }

        Type IWait.NeedType
        {
            get
            {
                if (this.rest != null)
                {
                    var method = this.rest.Method;
                    var parameters = method.GetParameters();
                    var itemType = parameters[1].ParameterType;
                    var type = itemType.GenericTypeArguments.Single();
                    return type;
                }
                else
                {
                    return null;
                }
            }
        }

        Delegate IWait.Rest
        {
            get
            {
                return this.rest;
            }
        }

        Type IWait.ItemType
        {
            get
            {
                return typeof(T);
            }
        }

        async Task<IWait> IWait.PollAsync(IFiber fiber)
        {
            this.ValidateNeed(Need.Poll);

            this.need = Need.Call;
            try
            {
                return await this.rest(fiber, this);
            }
            finally
            {
                this.need = Need.Done;
            }
        }

        void IWait.Post<D>(D item)
        {
            this.ValidateNeed(Need.Wait);

            var post = this as IPost<D>;
            if (post == null)
            {
                if (typeof(D).IsValueType)
                {
                    var postBoxed = this as IPost<object>;
                    if (postBoxed != null)
                    {
                        post = new PostStruct<D>(postBoxed);
                    }
                }
            }

            if (post == null)
            {
                IWait wait = this;
                wait.Fail(new InvalidTypeException(this, typeof(D)));
            }
            else
            {
                post.Post(item);
            }
        }

        void IWait.Fail(Exception fail)
        {
            this.ValidateNeed(Need.Wait);

            this.item = default(T);
            this.fail = fail;
            this.need = Need.Poll;
        }

        void IPost<T>.Post(T item)
        {
            this.ValidateNeed(Need.Wait);

            this.item = item;
            this.fail = null;
            this.need = Need.Poll;
        }

        void IWait<T>.Wait(Rest<T> rest)
        {
            this.ValidateNeed(Need.None);

            SetField.NotNull(out this.rest, nameof(rest), rest);
            this.need = Need.Wait;
        }

        IAwaiter<T> IAwaitable<T>.GetAwaiter()
        {
            return this;
        }

        bool IAwaiter<T>.IsCompleted
        {
            get
            {
                return this.need == Need.Call;
            }
        }

        T IAwaiter<T>.GetResult()
        {
            if (this.fail != null)
            {
                throw this.fail;
            }
            else
            {
                return this.item;
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        private void ValidateNeed(Need need)
        {
            if (need != this.need)
            {
                throw new InvalidNeedException(this, need);
            }
        }
    }

    public interface IWaitFactory
    {
        IWait<T> Make<T>();
    }

    [Serializable]
    public sealed class WaitFactory : IWaitFactory
    {
        IWait<T> IWaitFactory.Make<T>()
        {
            return new Wait<T>();
        }
    }
}
