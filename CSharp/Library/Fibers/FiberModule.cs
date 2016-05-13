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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Autofac;
using Autofac.Core;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    /// <summary>
    /// Autofac module for Fiber components.
    /// </summary>
    public abstract class FiberModule : Autofac.Module
    {
        public static readonly object Key_DoNotSerialize = new object();
        public static readonly object Key_SurrogateProvider = new object();

        /// <summary>
        /// Eagerly enumerate the services keyed with <see cref="Key_DoNotSerialize"/> that will not be serialized.
        /// </summary>
        /// <remarks>
        /// Services marked with <see cref="Key_DoNotSerialize"/> will not serialize their dependencies either.
        /// </remarks>
        private static IEnumerable<object> DoNotSerialize(IComponentContext context, IEnumerable<Parameter> parameters)
        {
            foreach (var registration in context.ComponentRegistry.Registrations)
            {
                foreach (var service in registration.Services)
                {
                    var keyed = service as KeyedService;
                    if (keyed != null)
                    {
                        if (keyed.ServiceKey == Key_DoNotSerialize)
                        {
                            object instance;
                            if (context.TryResolveService(service, parameters, out instance))
                            {
                                yield return instance;
                            }
                        }
                    }
                }
            }
        }

        private sealed class Resolver : Serialization.StoreInstanceByTypeSurrogate.IResolver
        {
            private readonly Dictionary<Type, object> instanceByType;
            public Resolver(IEnumerable<object> instances)
            {
                this.instanceByType = instances.ToDictionary(o => o.GetType(), o => o);
            }
            bool Serialization.StoreInstanceByTypeSurrogate.IResolver.Handles(Type type)
            {
                return this.instanceByType.ContainsKey(type);
            }

            object Serialization.StoreInstanceByTypeSurrogate.IResolver.Resolve(Type type)
            {
                var instance = instanceByType[type];
                return instance;
            }
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // singleton components

            builder
                .RegisterType<DefaultTraceListener>()
                .As<TraceListener>()
                .SingleInstance();

            builder
                .Register(c => Comparer<double>.Default)
                .As<IComparer<double>>();

            builder
                .RegisterType<NormalizedTraits>()
                .As<ITraits<double>>()
                .SingleInstance();

            builder
                .Register(c => new Serialization.StoreInstanceByTypeSurrogate(priority: int.MaxValue))
                .Keyed<Serialization.ISurrogateProvider>(Key_SurrogateProvider)
                .SingleInstance();

            builder
                .Register(c => new Serialization.ClosureCaptureErrorSurrogate(priority: 1))
                .Keyed<Serialization.ISurrogateProvider>(Key_SurrogateProvider)
                .SingleInstance();

            builder
                .Register(c => new Serialization.JObjectSurrogate(priority: 3))
                .Keyed<Serialization.ISurrogateProvider>(Key_SurrogateProvider)
                .SingleInstance();

            builder
                .RegisterDecorator<Serialization.ISurrogateProvider>((c, inner) => new Serialization.SurrogateLogDecorator(inner, c.Resolve<TraceListener>()), fromKey: Key_SurrogateProvider);

            builder
                .RegisterType<Serialization.SurrogateSelector>()
                .As<ISurrogateSelector>()
                .SingleInstance();

            // per request, depends on resolution parameters through "p"

            builder
                .Register((c, p) => new Resolver(DoNotSerialize(c, p).Distinct().ToArray()))
                .As<Serialization.StoreInstanceByTypeSurrogate.IResolver>()
                .InstancePerLifetimeScope();

            builder
                .Register((c, p) => new BinaryFormatter(c.Resolve<ISurrogateSelector>(), new StreamingContext(StreamingContextStates.All, c.Resolve<Serialization.StoreInstanceByTypeSurrogate.IResolver>(p))))
                .As<IFormatter>()
                .InstancePerLifetimeScope();
        }
    }

    /// <summary>
    /// Autofac module for Fiber components.
    /// </summary>
    public sealed class FiberModule<C> : FiberModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // [Serializable] components, serialized as type, deserialized through container resolution of type

            builder
                .RegisterType<WaitFactory<C>>()
                .Keyed<IWaitFactory<C>>(Key_DoNotSerialize)
                .As<IWaitFactory<C>>()
                .SingleInstance();

            builder
                .RegisterType<FrameFactory<C>>()
                .Keyed<IFrameFactory<C>>(Key_DoNotSerialize)
                .As<IFrameFactory<C>>()
                .SingleInstance();

            // per request, no resolution parameter dependency

            builder
                .RegisterType<Fiber<C>>()
                .As<IFiberLoop<C>>()
                .InstancePerLifetimeScope();

            builder
                .Register((c, p) => new FactoryStore<IFiberLoop<C>>(new ErrorResilientStore<IFiberLoop<C>>(new FormatterStore<IFiberLoop<C>>(c.Resolve<Stream>(p), c.Resolve<IFormatter>(p))), c.Resolve<Func<IFiberLoop<C>>>(p)))
                .As<IStore<IFiberLoop<C>>>()
                .InstancePerLifetimeScope();
        }
    }

    public sealed class ReflectionSurrogateModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
                .Register(c => new Serialization.StoreInstanceByFieldsSurrogate(priority: 2))
                .Keyed<Serialization.ISurrogateProvider>(FiberModule.Key_SurrogateProvider)
                .SingleInstance();
        }
    }
}