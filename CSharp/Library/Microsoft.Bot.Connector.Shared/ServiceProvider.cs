using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Bot.Connector
{
    public sealed class ServiceProvider
    {
        private static ServiceProvider instance;
        private readonly IServiceProvider provider;

        private ServiceProvider(IServiceProvider provider)
        {
            this.provider = provider;
        }

        /// <summary>
        /// Gets the currently registered instance of the <see cref="ServiceProvider"/>.
        /// </summary>
        public static ServiceProvider Instance
        {
            get
            {
                ThrowOnNullInstance();
                return ServiceProvider.instance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the service provider is registered.
        /// </summary>
        public static bool IsRegistered
        {
            get
            {
                return ServiceProvider.instance != null;
            }
        }

        /// <summary>
        /// Gets the configuration root instance.
        /// </summary>
        public IConfigurationRoot ConfigurationRoot
        {
            get
            {
                return this.GetService<IConfigurationRoot>();
            }
        }

        /// <summary>
        /// Registers the <see cref="IServiceProvider"/> instance.
        /// </summary>
        /// <param name="provider">The service provider instance.</param>
        public static void RegisterServiceProvider(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (ServiceProvider.instance == null)
            {
                ServiceProvider.instance = new ServiceProvider(provider);
            }
            else
            {
                // so we don't worry about race-conditions on ServiceProvider.instance
                throw new InvalidOperationException("The service provider can only be registered once during the AppDomain lifecycle");
            }
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <returns>A new logger instance.</returns>
        public ILogger CreateLogger()
        {
            return this.GetService<ILoggerFactory>().CreateLogger("Microsoft.Bot.Connector");
        }

        private static void ThrowOnNullInstance()
        {
            if (ServiceProvider.instance == null)
            {
                throw new InvalidOperationException("The service provider instance was not register. Please call RegisterServiceProvider before using ServiceProvider.Instance.");
            }
        }

        private TService GetService<TService>() where TService : class
        {
            Type serviceType = typeof(TService);
            TService service = this.provider.GetService(serviceType) as TService;

            if (service == null)
            {
                throw new InvalidOperationException($"The service \"{serviceType.FullName}\" is missing on the registered service provider. This usually means that the missing service is not available in the current platform.");
            }

            return service;
        }
    }
}