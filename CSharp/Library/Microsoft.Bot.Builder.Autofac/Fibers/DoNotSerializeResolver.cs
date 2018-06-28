// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Bot Framework: http://botframework.com
// 
// Bot Builder SDK GitHub:
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Core;
using Microsoft.Bot.Builder.Scorables.Internals;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    public sealed class DoNotSerializeResolver : IResolver
    {
        private readonly IComponentContext context;
        private readonly IEnumerable<Parameter> parameters;
        public DoNotSerializeResolver(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            SetField.NotNull(out this.context, nameof(context), context);
            SetField.NotNull(out this.parameters, nameof(parameters), parameters);
        }

        public static IEnumerable<Type> Services(Type root)
        {
            var types = new Queue<Type>();
            types.Enqueue(root);

            while (types.Count > 0)
            {
                var type = types.Dequeue();
                if (type == typeof(MulticastDelegate))
                {
                    continue;
                }

                yield return type;

                foreach (var next in type.GetInterfaces())
                {
                    types.Enqueue(next);
                }

                while (type.BaseType != null)
                {
                    type = type.BaseType;
                    types.Enqueue(type);
                }
            }
        }

        public static readonly ConcurrentDictionary<Type, IReadOnlyList<Type>> ServicesByType = new ConcurrentDictionary<Type, IReadOnlyList<Type>>();

        // cache KeyedService to avoid Autofac memory leak
        private static readonly ConcurrentDictionary<Type, KeyedService> ItemKeyedServiceByType = new ConcurrentDictionary<Type, KeyedService>();
        private static readonly ConcurrentDictionary<Type, KeyedService> NullKeyedServiceByType = new ConcurrentDictionary<Type, KeyedService>();

        bool IResolver.TryResolve(Type type, object tag, out object value)
        {
            if (tag == null)
            {
                var services = ServicesByType.GetOrAdd(type, t => Services(t).Distinct().ToArray());
                for (int index = 0; index < services.Count; ++index)
                {
                    var serviceType = services[index];
                    var service = ItemKeyedServiceByType.GetOrAdd(serviceType, t => new KeyedService(FiberModule.Key_DoNotSerialize, t));

                    var registry = this.context.ComponentRegistry;

                    IComponentRegistration registration;
                    if (registry.TryGetRegistration(service, out registration))
                    {
                        // Autofac will still generate "implicit relationship types" (e.g. Func or IEnumerable)
                        // and ignore the key in KeyedService
                        if (IsFunc(serviceType))
                        {
                            var keyedService = NullKeyedServiceByType.GetOrAdd(serviceType, t => new KeyedService(new object(), t));
                            bool generated = registry.IsRegistered(keyedService);
                            if (generated)
                            {
                                continue;
                            }
                        }
                        else if (IsAutoFacImplicit(serviceType))
                        {
                            continue;
                        }

                        value = this.context.ResolveComponent(registration, this.parameters);
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        private static bool IsFunc(Type serviceType)
        {
            if (typeof(Delegate).IsAssignableFrom(serviceType))
            {
                if (serviceType.IsGenericType)
                {
                    var definition = serviceType.GetGenericTypeDefinition();
                    if (definition.Namespace == typeof(Func<>).Namespace && definition.Name.StartsWith("Func"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        private static bool IsAutoFacImplicit(Type serviceType)
        {
            if (IsFunc(serviceType))
            {
                return true;
            }
            if (serviceType.IsArray)
            {
                return true;
            }
            if (serviceType.IsGenericType)
            {
                var genericType = serviceType.GetGenericTypeDefinition();
                if (genericType == typeof(IEnumerable<>) ||
                    genericType == typeof(ICollection<>) ||
                    genericType == typeof(IList<>) ||
                    genericType == typeof(IReadOnlyCollection<>) ||
                    genericType == typeof(IReadOnlyList<>))
                {
                    // always serialize collections
                    return true;
                }
            }
            return false;
        }
    }
}