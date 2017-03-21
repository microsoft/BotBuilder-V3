using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Bot.Connector
{
    /// <summary>
    /// Implements the <see cref="IServiceProvider"/> used by the common bot connector for platform specific service discovery.
    /// </summary>
    public sealed class BotServiceProvider : IServiceProvider
    {
        private static readonly ReadOnlyDictionary<Type, object> Services;

        static BotServiceProvider()
        {            
            // add here the services this bot connector implementation supports
            var services = new Dictionary<Type, object>();
            
            // Configuration from AppSettings plus environment variables
            services.Add(typeof(IConfigurationRoot), 
                new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddInMemoryCollection(ConfigurationManager.AppSettings.AllKeys.Select(k => new KeyValuePair<string, string>(k, ConfigurationManager.AppSettings[k])))
                .Build());

            // Logging
            services.Add(typeof(ILoggerFactory), new LoggerFactory()
                .AddTraceSource("Microsoft.Bot.Connector"));

            Services = new ReadOnlyDictionary<Type, object>(services);

            // Auto register with base implementation
            if (!ServiceProvider.IsRegistered)
            {
                ServiceProvider.RegisterServiceProvider(new BotServiceProvider());
            }
        }

        public static ServiceProvider Instance { get { return ServiceProvider.Instance; } }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type serviceType.-or- null if there is no service object
        /// of type serviceType.</returns>
        public object GetService(Type serviceType)
        {
            object service = null;
            Services.TryGetValue(serviceType, out service);
            return service;
        }
    }
}
