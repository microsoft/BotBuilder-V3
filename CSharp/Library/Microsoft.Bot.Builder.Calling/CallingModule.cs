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
