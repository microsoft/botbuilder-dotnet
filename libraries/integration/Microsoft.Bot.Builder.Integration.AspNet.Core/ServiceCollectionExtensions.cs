// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Defines extension methods for to add common services to the application's service collection<see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds bot related services to the application's service collection.
        /// </summary>
        /// <remark>
        /// The following dependencies are added with TrySingleton so advanced scenarios can override them to customize the runtime behavior:
        /// <see cref="BotFrameworkAuthentication"/>,
        /// <see cref="IBotFrameworkHttpAdapter"/>,
        /// <see cref="IStorage"/>,
        /// <see cref="UserState"/>,
        /// <see cref="ConversationState"/>
        /// 
        /// This set of dependencies is designed to be sufficient to run a typical bot. Since each of these
        /// are registered using TrySingleton to provide a different implementation of any of the dependencies
        /// just register them with the service collection before calling AddBotRuntime.
        /// 
        /// </remark>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void AddBotRuntime(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterCommonServices(services, configuration);
        }

        /// <summary>
        /// Overload that allows for creating the Bot (IBot) type as well as other startup types.
        /// </summary>
        /// <typeparam name="TBot">Type of Bot.</typeparam>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void AddBotRuntime<TBot>(this IServiceCollection services, IConfiguration configuration) 
            where TBot : class, IBot
        {
            RegisterCommonServices(services, configuration);

            services.AddSingleton<IBot, TBot>();
        }

        /// <summary>
        /// Overload that allows for providing the Bot (IBot) type, and the main Dialog type as well as other startup types.
        /// </summary>
        /// <typeparam name="TBot">Type of Bot.</typeparam>
        /// <typeparam name="TDialog">Type of Dialog.</typeparam>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">The application configuration.</param>
        public static void AddBotRuntime<TBot, TDialog>(this IServiceCollection services, IConfiguration configuration)
            where TBot : class, IBot
            where TDialog : Dialog
        {
            RegisterCommonServices(services, configuration);
            services.AddSingleton<TDialog>();
            services.AddSingleton<IBot, TBot>();
        }

        private static void RegisterCommonServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            // Ensure the IConfiguration is available. (Azure Functions don't do this.)
            services.TryAddSingleton(configuration);

            // Add basic authentication.
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

            // Add a CoreBotAdapter as the IBotFrameworkHttpAdapter unless one was already registered.
            services.TryAddSingleton<IBotFrameworkHttpAdapter, CoreBotAdapter>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.TryAddSingleton<IStorage>(new MemoryStorage());

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.TryAddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.TryAddSingleton<ConversationState>();
        }
    }
}
