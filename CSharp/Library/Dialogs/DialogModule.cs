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
using System.IO;
using System.Text;

using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

using Autofac;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// Autofac module for Dialog components.
    /// </summary>
    public sealed class DialogModule : Autofac.Module
    {
        public const string BlobKey = "DialogState";
        public static readonly object LifetimeScopeTag = typeof(DialogModule);
        public static ILifetimeScope BeginLifetimeScope(ILifetimeScope scope, Message message)
        {
            var inner = scope.BeginLifetimeScope(LifetimeScopeTag);
            inner.Resolve<Message>(TypedParameter.From(message));
            return inner;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new FiberModule<DialogTask>());

            // every lifetime scope is driven by a message

            builder
                .Register((c, p) => p.TypedAs<Message>())
                .AsSelf()
                .InstancePerMatchingLifetimeScope(LifetimeScopeTag);

            // components not marked as [Serializable]

            builder
                .Register(c => new DetectEmulatorFactory(c.Resolve<Message>(), new Uri("http://localhost:9000")))
                .As<IConnectorClientFactory>()
                .InstancePerLifetimeScope();

            builder
                .Register(c => c.Resolve<IConnectorClientFactory>().Make())
                .As<IConnectorClient>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<JObjectBotData>()
                .As<IBotData>()
                .InstancePerLifetimeScope();

            builder
                .Register(c => new BotDataBagStream(c.Resolve<IBotData>().PerUserInConversationData, BlobKey))
                .As<Stream>()
                .InstancePerLifetimeScope();

            builder
                .Register(c => new ScoringDialogTask<double>(
                                new LocalizedDialogTask(
                                    new DialogTask(c.Resolve<Func<IDialogContext>>(), c.Resolve<IStore<IFiberLoop<DialogTask>>>())
                                    ),
                                c.Resolve<IComparer<double>>(), c.Resolve<ITraits<double>>(), c.Resolve<IScorable<double>[]>()))
                .As<IDialogTask>()
                .As<IDialogStack>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<SendLastInline_BotToUser>()
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<DialogContext>()
                .As<IDialogContext>()
                .InstancePerLifetimeScope();
        }
    }
}
