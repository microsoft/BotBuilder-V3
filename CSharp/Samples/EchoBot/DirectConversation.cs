using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.Bot.Sample.EchoBot
{
    public static partial class DirectConversation
    {
        public static async Task<Message> SendDirectAsync(Message toBot, Func<IDialog<object>> MakeRoot, CancellationToken token = default(CancellationToken))
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new DialogModule_MakeRoot());

            builder
                .RegisterType<AlwaysSendDirect_BotToUser>()
                .AsSelf()
                .As<IBotToUser>()
                .InstancePerLifetimeScope();

            var container = builder.Build();

            using (var scope = DialogModule.BeginLifetimeScope(container, toBot))
            {
                DialogModule_MakeRoot.Register(scope, MakeRoot);

                using (new LocalizedScope(toBot.Language))
                {
                    var task = scope.Resolve<IPostToBot>();
                    await task.PostAsync(toBot, default(CancellationToken));
                    return null;
                }
            }
        }
    }
}