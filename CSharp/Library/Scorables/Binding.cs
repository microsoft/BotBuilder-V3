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

using Microsoft.Bot.Builder.Internals.Fibers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Internals.Scorables
{
    /// <summary>
    /// Represents a binding of arguments to a method's parameters.
    /// </summary>
    public sealed class Binding : IEquatable<Binding>
    {
        private readonly MethodInfo method;
        private readonly IReadOnlyList<ParameterInfo> parameters;
        private readonly object instance;
        private readonly object[] arguments;
        public Binding(MethodInfo method, IReadOnlyList<ParameterInfo> parameters, object instance, object[] arguments)
        {
            SetField.NotNull(out this.method, nameof(method), method);
            SetField.NotNull(out this.parameters, nameof(parameters), parameters);
            SetField.NotNull(out this.instance, nameof(instance), instance);
            SetField.NotNull(out this.arguments, nameof(arguments), arguments);
        }
        public Task InvokeAsync(CancellationToken token)
        {
            try
            {
                // late-bound provide the CancellationToken
                for (int index = 0; index < this.parameters.Count; ++index)
                {
                    var type = this.parameters[index].ParameterType;
                    bool cancel = type.IsAssignableFrom(typeof(CancellationToken));
                    if (cancel)
                    {
                        this.arguments[index] = token;
                    }
                }

                var result = this.method.Invoke(this.instance, this.arguments);
                // if the result is a task, wait for its completion and propagate any exceptions
                var task = result as Task;
                if (task != null)
                {
                    return task;
                }

                return Task.CompletedTask;
            }
            catch (OperationCanceledException error)
            {
                return Task.FromCanceled(error.CancellationToken);
            }
            catch (Exception error)
            {
                return Task.FromException(error);
            }
        }
        public override string ToString()
        {
            return this.method.ToString();
        }

        public override int GetHashCode()
        {
            return this.method.GetHashCode();
        }

        public override bool Equals(object other)
        {
            IEquatable<Binding> equatable = this;
            return equatable.Equals(other as Binding);
        }

        bool IEquatable<Binding>.Equals(Binding other)
        {
            return other != null
                && object.Equals(this.method, other.method)
                && this.parameters.Equals(other.parameters)
                && object.Equals(this.instance, other.instance)
                && this.arguments.Equals(other.arguments);
        }

        public sealed class ResolutionComparer : IComparer<Binding>
        {
            public static readonly IComparer<Binding> Instance = new ResolutionComparer();
            private ResolutionComparer()
            {
            }
            private bool TryCompareParameterTypeAssignability(Binding one, Binding two, int index, out int compare)
            {
                var l = one.parameters[index].ParameterType;
                var r = two.parameters[index].ParameterType;
                if (l.Equals(r))
                {
                    compare = 0;
                    return true;
                }
                if (l.IsAssignableFrom(r))
                {
                    compare = -1;
                    return true;
                }
                else if (r.IsAssignableFrom(l))
                {
                    compare = +1;
                    return true;
                }

                compare = 0;
                return false;
            }

            public static int UpdateComparisonConsistently(Binding one, Binding two, int compareOld, int compareNew)
            {
                if (compareOld == 0)
                {
                    return compareNew;
                }
                else if (compareNew == 0)
                {
                    return compareOld;
                }
                else if (compareNew == compareOld)
                {
                    return compareOld;
                }
                else
                {
                    throw new MethodResolutionException("inconsistent parameter overrides", one, two);
                }
            }

            int IComparer<Binding>.Compare(Binding one, Binding two)
            {
                if (one.method.ReturnType != two.method.ReturnType)
                {
                    throw new MethodResolutionException("inconsistent return types", one, two);
                }

                int compare = 0;

                var count = Math.Min(one.parameters.Count, two.parameters.Count);
                for (int index = 0; index < count; ++index)
                {
                    int parameter;
                    if (TryCompareParameterTypeAssignability(one, two, index, out parameter))
                    {
                        compare = UpdateComparisonConsistently(one, two, compare, parameter);
                    }
                    else
                    {
                        throw new MethodResolutionException("inconsistent parameter types", one, two);
                    }
                }

                int length = one.parameters.Count.CompareTo(two.parameters.Count);
                compare = UpdateComparisonConsistently(one, two, compare, length);
                return compare;
            }
        }

        [Serializable]
        public sealed class MethodResolutionException : Exception
        {
            public MethodResolutionException(string message, Binding one, Binding two)
                : base($"{message}: {one.method} and {two.method}")
            {
            }
            private MethodResolutionException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
            }
        }
    }
}
