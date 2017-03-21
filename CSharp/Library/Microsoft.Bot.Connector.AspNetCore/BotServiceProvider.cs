using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Implements the <see cref="IServiceProvider"/> used by the common bot connector for platform specific service discovery.
    /// </summary>
    public sealed class BotServiceProvider : IServiceProvider
    {
        internal static IServiceCollection serviceCollection;

        internal static IServiceProvider ServiceProvider
        {
            get
            {
                if (serviceCollection == null)
                {
                    throw new InvalidOperationException($"{nameof(serviceCollection)} is not defined. Please call services.UseBotConnector() in your ASP.NET Core Startup Configure method, where \"services\" is your instance of IServiceProvider.");
                }

                return serviceCollection.BuildServiceProvider();
            }
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType.-or- null if there is no service object
        /// of type serviceType.</returns>
        public object GetService(Type serviceType)
        {
            return ServiceProvider.GetService(serviceType);
        }
    }

    public static class BotServiceProviderExtensions
    {
        public static void UseBotConnector(this IServiceCollection serviceCollection)
        {
            BotServiceProvider.serviceCollection = serviceCollection;

            // manually register this with the base connector assembly to support ASPNET CORE without BotBuilder (and autofac)
            if (!ServiceProvider.IsRegistered)
            {
                ServiceProvider.RegisterServiceProvider(new BotServiceProvider());
            }
        }
    }
}
