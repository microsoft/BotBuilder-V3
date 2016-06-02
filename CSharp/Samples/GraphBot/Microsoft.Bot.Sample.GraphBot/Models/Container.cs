using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Sample.GraphBot.Models
{
    public static class Container
    {
        public static readonly IContainer Instance;
        static Container()
        {
            var builder = new ContainerBuilder();

            // include the default Dialog module registration
            builder.RegisterModule(new DialogModule());

            // add the Microsoft.Graph IAuthenticationProvider interface, which depends on the client keys and our
            // AAD token storage.
            builder
                .Register(c => new AuthenticationProvider(c.Resolve<IClientKeys>(), c.Resolve<IBotData>().UserData))
                .As<IAuthenticationProvider>()
                .InstancePerLifetimeScope();

            // register the default implementation of the IGraphServiceClient - don't bother serializing it with the dialog state.
            builder
                .RegisterType<GraphServiceClient>()
                .Keyed<IGraphServiceClient>(FiberModule.Key_DoNotSerialize)
                .As<IGraphServiceClient>()
                .InstancePerLifetimeScope();

            // show the container how to make a ResumptionCookie from a Message
            builder
                .Register(c => new ResumptionCookie(c.Resolve<Connector.Message>()))
                .AsSelf();

            // register a root dialog to be resolved through the container, so that it may get its dependencies from the container.
            builder
                .Register<Func<IDialog<object>>>(c =>
                {
                    var scope = c.Resolve<ILifetimeScope>();
                    return () => new RootDialog(scope.Resolve<IGraphServiceClient>(), scope.Resolve<Uri>(), scope.Resolve<ResumptionCookie>());
                })
                .AsSelf()
                .InstancePerLifetimeScope();

            // use a hacky method to pass data from ASP.NET container to Bot Builder container
            builder
                .Register<Uri>((c, p) => p.TypedAs<Uri>())
                .AsSelf()
                .InstancePerLifetimeScope();
            builder
                .Register<IClientKeys>((c, p) => p.TypedAs<IClientKeys>())
                .AsSelf()
                .InstancePerLifetimeScope();

            Instance = builder.Build();
        }
    }
}
