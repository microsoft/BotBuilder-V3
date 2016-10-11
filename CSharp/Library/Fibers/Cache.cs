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
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    public interface ICache<C, K, V>
    {
        Task<V> GetOrAddAsync(C context, K key, Func<C, K, CancellationToken, Task<V>> make, CancellationToken token);
    }

    public sealed class NullCache<C, K, V> : ICache<C, K, V>
    {
        Task<V> ICache<C, K, V>.GetOrAddAsync(C context, K key, Func<C, K, CancellationToken, Task<V>> make, CancellationToken token)
        {
            try
            {
                return make(context, key, token);
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<V>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException<V>(error);
            }
        }
    }

    [Serializable]
    public sealed class DictionaryCache<C, K, V> : ICache<C, K, V>, ISerializable
    {
        private readonly IEqualityComparer<K> comparer;

        [NonSerialized]
        private readonly Dictionary<K, Task<V>> cache;
        public DictionaryCache(IEqualityComparer<K> comparer)
        {
            SetField.NotNull(out this.comparer, nameof(comparer), comparer);
            this.cache = new Dictionary<K, Task<V>>(comparer);
        }
        private DictionaryCache(SerializationInfo info, StreamingContext context)
        {
            SetField.NotNullFrom(out this.comparer, nameof(this.comparer), info);
            this.cache = new Dictionary<K, Task<V>>(comparer);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.comparer), comparer);
        }

        Task<V> ICache<C, K, V>.GetOrAddAsync(C context, K key, Func<C, K, CancellationToken, Task<V>> make, CancellationToken token)
        {
            try
            {
                Task<V> task;
                lock (this.cache)
                {
                    if (!this.cache.TryGetValue(key, out task))
                    {
                        // TODO: properly propagate the CancellationToken
                        task = make(context, key, token);
                        this.cache.Add(key, task);
                    }
                }

                return task;
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled<V>(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException<V>(error);
            }
        }
    }
}
