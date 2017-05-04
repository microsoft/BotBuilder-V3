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

using Autofac;
using System;
using System.Net.Http;

namespace Microsoft.Bot.Builder.Calling
{
    /// <summary>
    /// Autofac module for Calling components.
    /// </summary>
    public sealed class CallingModule : Module
    {
        public static readonly object LifetimeScopeTag = typeof(CallingModule);

        public static ILifetimeScope BeginLifetimeScope(ILifetimeScope scope, HttpRequestMessage request)
        {
            var inner = scope.BeginLifetimeScope(LifetimeScopeTag);
            inner.Resolve<HttpRequestMessage>(TypedParameter.From(request));
            return inner;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder
               .Register((c, p) => p.TypedAs<HttpRequestMessage>())
               .AsSelf()
               .InstancePerMatchingLifetimeScope(LifetimeScopeTag);

            builder
               .RegisterType<CallingContext>()
               .AsSelf()
               .InstancePerMatchingLifetimeScope(LifetimeScopeTag);

            builder
                .Register(c => CallingBotServiceSettings.LoadFromCloudConfiguration())
                .AsSelf()
                .SingleInstance();

            builder
                .Register(c => new CallingBotService(c.Resolve<CallingBotServiceSettings>()))
                .AsSelf()
                .As<ICallingBotService>()
                .SingleInstance();

        }
    }

    public sealed class CallingModule_MakeBot : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new CallingModule());

            // First call to handler will register single instance of the calling bot
            // it can be changed to create a new instance per matching lifetime
            builder
                .Register((c, p) => p.TypedAs<Func<ICallingBotService, ICallingBot>>())
                .AsSelf()
                .SingleInstance();
            //.InstancePerMatchingLifetimeScope(CallingModule.LifetimeScopeTag);

            builder
                .Register(c =>
                   {
                       var makeBot = c.Resolve<Func<ICallingBotService, ICallingBot>>();
                       var callingBotService = c.Resolve<ICallingBotService>();
                       return makeBot(callingBotService);
                   }
                )
                .As<ICallingBot>()
                .SingleInstance();
            //.InstancePerMatchingLifetimeScope(CallingModule.LifetimeScopeTag);
        }

        public static void Register(ILifetimeScope scope, Func<ICallingBotService, ICallingBot> MakeCallingBot)
        {
            scope.Resolve<Func<ICallingBotService, ICallingBot>>(TypedParameter.From(MakeCallingBot));
        }
    }
}
