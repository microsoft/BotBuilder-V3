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
using System.Diagnostics;
using Microsoft.Bot.Builder.Internals.Fibers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Bot.Connector;
using Autofac.Core;

namespace Microsoft.Bot.Builder.Dialogs.Internals
{
    /// <summary>
    /// Autofac module for Dialog components.
    /// </summary>
    public sealed class DialogModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterModule(new FiberModule());

            // per-message
            // http://stackoverflow.com/questions/1211595/autofac-parameter-passing-and-autowiring

            builder
                .Register((c, p) => new DetectEmulatorFactory(p.TypedAs<Message>(), new Uri("http://localhost:9000")))
                .As<IConnectorClientFactory>()
                .InstancePerLifetimeScope();

            builder
                .Register((c, p) => c.Resolve<IConnectorClientFactory>(p).Make())
                .As<IConnectorClient>()
                .InstancePerLifetimeScope();

            builder
                .RegisterType<JObjectBotData>()
                .Keyed<IBotData>(FiberModule.Key_DoNotSerialize)
                .As<IBotData>()
                .InstancePerLifetimeScope();

            builder
                .Register((c, p) => new SendLastInline_BotToUser(p.TypedAs<Message>(), c.Resolve<IConnectorClient>(p)))
                .Keyed<IBotToUser>(FiberModule.Key_DoNotSerialize)
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            const string BlobKey = "DialogState";

            builder
                .Register((c, p) => new DialogContextFactory(new ErrorResilientDialogContextStore(new DialogContextStore(c.Resolve<IFormatter>(p), c.Resolve<IBotData>(p), BlobKey)), c.Resolve<IFrameFactory>(), c.Resolve<IBotToUser>(p), c.Resolve<IBotData>(p)))
                .As<IDialogContextStore>()
                .InstancePerLifetimeScope();
        }
    }
}
