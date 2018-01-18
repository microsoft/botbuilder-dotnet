using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Bot.Connector
{
    public sealed class BotServiceProvider : IServiceProvider
    {
        private static object gate = new object(); 
        internal static IServiceCollection serviceCollection;
        private static BotServiceProvider instance;
        private readonly IServiceProvider serviceProvider;

        private BotServiceProvider()
        {
            if (serviceCollection == null)
            {
                throw new InvalidOperationException($"{nameof(serviceCollection)} is not defined. Please call services.UseBotConnector() in your ASP.NET Core Startup Configure method, where \"services\" is your instance of IServiceProvider.");
            }

            this.serviceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Gets the currently registered instance of the <see cref="ServiceProvider"/>.
        /// </summary>
        public static BotServiceProvider Instance
        {
            get
            {
                lock (gate)
                {
                    if (instance == null)
                    {
                        instance = new BotServiceProvider();
                    }
                    return BotServiceProvider.instance;
                }
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
            return serviceProvider.GetService(serviceType);
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/> instance.
        /// </summary>
        /// <returns>A new logger instance.</returns>
        public ILogger CreateLogger()
        {
            return this.GetService<ILoggerFactory>().CreateLogger("Microsoft.Bot.Connector");
        }

        /// <summary>
        /// Gets the configuration root instance.
        /// </summary>
        public IConfiguration ConfigurationRoot
        {
            get
            {
                return this.GetService<IConfiguration>();
            }
        }

        private TService GetService<TService>() where TService : class
        {
            Type serviceType = typeof(TService);
            TService service = serviceProvider.GetService(serviceType) as TService;

            if (service == null)
            {
                throw new InvalidOperationException($"The service \"{serviceType.FullName}\" is missing on the registered service provider. This usually means that the missing service is not available in the current platform.");
            }

            return service;
        }
    }

    public static class BotServiceProviderExtensions
    {
        public static void UseBotConnector(this IServiceCollection serviceCollection)
        {
            BotServiceProvider.serviceCollection = serviceCollection;
        }
    }
}