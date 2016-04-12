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
using System.Text;
using System.Threading.Tasks;

using Autofac;
using Autofac.Core;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Microsoft.Bot.Builder.Internals.Fibers
{
    /// <summary>
    /// Autofac module for Fiber components.
    /// </summary>
    public sealed class FiberModule : Autofac.Module
    {
        public static readonly object Key_DoNotSerialize = new object();
        public static readonly object Key_SurrogateProvider = new object();

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

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            // singletons

            builder
                .RegisterType<DefaultTraceListener>()
                .As<TraceListener>()
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
                .RegisterDecorator<Serialization.ISurrogateProvider>((c, inner) => new Serialization.SurrogateLogDecorator(inner, c.Resolve<TraceListener>()), fromKey: Key_SurrogateProvider);

            builder
                .RegisterType<Serialization.SurrogateSelector>()
                .As<ISurrogateSelector>()
                .SingleInstance();

            builder
                .RegisterType<WaitFactory>()
                .Keyed<IWaitFactory>(Key_DoNotSerialize)
                .As<IWaitFactory>()
                .SingleInstance();

            builder
                .RegisterType<FrameFactory>()
                .Keyed<IFrameFactory>(Key_DoNotSerialize)
                .As<IFrameFactory>()
                .SingleInstance();

            // per request
            builder
                .Register((c, p) => new Serialization.SimpleServiceLocator(DoNotSerialize(c, p).Distinct().ToArray()))
                .As<IServiceProvider>()
                .InstancePerLifetimeScope();

            builder
                .Register((c, p) => new BinaryFormatter(c.Resolve<ISurrogateSelector>(), new StreamingContext(StreamingContextStates.All, c.Resolve<IServiceProvider>(p))))
                .As<IFormatter>()
                .InstancePerLifetimeScope();

            builder
                .Register((c, p) => new Fiber(c.Resolve<IFrameFactory>(p)))
                .As<IFiberLoop>()
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