// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Bot.Builder.Extensions.DependencyInjection
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
        /// are registered using TrySingleton if you want to provide a different implementation of any of the dependencies
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
        /// are registered using TrySingleton if you want to provide a different implementation of any of the dependencies
        /// just register them with the service collection before calling AddBotRuntime.
        /// 
        /// </remark>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <typeparam name="TBot">Type of Bot.</typeparam>
        public static void AddBotRuntime<TBot>(this IServiceCollection services, IConfiguration configuration)
            where TBot : class, IBot
        {
            RegisterCommonServices(services, configuration);

            services.AddSingleton<IBot, TBot>();
        }

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
        /// are registered using TrySingleton if you want to provide a different implementation of any of the dependencies
        /// just register them with the service collection before calling AddBotRuntime.
        /// 
        /// </remark>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <typeparam name="TBot">Type of Bot.</typeparam>
        /// <typeparam name="TDialog">Type of Dialog.</typeparam>
        public static void AddBotRuntime<TBot, TDialog>(this IServiceCollection services, IConfiguration configuration)
            where TBot : class, IBot
            where TDialog : Dialog
        {
            RegisterCommonServices(services, configuration);

            // Register the User state. (Used in Dialog implementation.)
            services.TryAddSingleton<UserState>();

            // Register the Conversation state. (Used in Dialog system itself.)
            services.TryAddSingleton<ConversationState>();

            // Register the root Dialog type.
            services.AddSingleton<TDialog>();
            services.AddSingleton<IBot, TBot>();
        }

        // The methods below create a Fluent style API for adding required services to the bot startup.

        /// <summary>
        /// Configures service collection to contain an IConfiguration instance.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <param name="configuration">The IConfiguration instance to register.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Ensure the IConfiguration is available. (Azure Functions don't do this.)
            services.TryAddSingleton(configuration);
            return services;
        }

        /// <summary>
        /// Configures service collection to use the default ConfigurationBotFrameworkAuthentication implementation.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotAuthentication(this IServiceCollection services)
        {
            // Register the default ConfigurationBotFrameworkAuthentication type.
            services.TryAddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            return services;
        }

        /// <summary>
        /// Configures service collection to use a user specified BotFrameworkAuthentication implementation.
        /// </summary>
        /// <typeparam name="T">The type of the user specified BotFrameworkAuthentication class.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotAuthentication<T>(this IServiceCollection services)
            where T : BotFrameworkAuthentication
        {
            // Register the user specified authentication type.
            services.TryAddSingleton<BotFrameworkAuthentication, T>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a default IBotFrameworkHttpAdapter based on the CoreBotAdapter implementation.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotHttpAdapter(this IServiceCollection services)
        {
            // Add a CoreBotAdapter as the IBotFrameworkHttpAdapter unless one was already registered.
            services.TryAddSingleton<IBotFrameworkHttpAdapter, CoreBotAdapter>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a user specified IBotFrameworkHttpAdapter implementation.
        /// </summary>
        /// <typeparam name="T">Type of the IBotFramworkHttpAdapter to register.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotHttpAdapter<T>(this IServiceCollection services)
            where T : class, IBotFrameworkHttpAdapter
        {
            // Add the user specified adapter as the IBotFrameworkHttpAdapter unless one was already registered.
            services.TryAddSingleton<IBotFrameworkHttpAdapter, T>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a user specified implementation of IStorage.
        /// </summary>
        /// <typeparam name="T">Type of the IStorage to register.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotStorage<T>(this IServiceCollection services)
            where T : class, IStorage
        {
            // Registers an IStorage implementation Type.
            services.TryAddSingleton<IStorage, T>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a UserState instance. This is required if you use Dialogs.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotUserState(this IServiceCollection services)
        {
            // Register the UserState. (Used in Dialog implementation.)
            services.TryAddSingleton<UserState>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a ConversationState instance. This is required if you use Dialogs.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <typeparam name="T">Type of the IStorage to register.</typeparam>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotState<T>(this IServiceCollection services)
            where T : BotState
        {
            // Register the State Type.
            services.TryAddSingleton<T>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a ConversationState instance. This is required if you use Dialogs.
        /// </summary>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotConversationState(this IServiceCollection services)
        {
            // Register the ConversationState. (Used in Dialog implementation.)
            services.TryAddSingleton<ConversationState>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a Dialog.
        /// </summary>
        /// <typeparam name="T">Type of the Dialog to register.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <param name="instance">The Dialog instance to register.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotDialog<T>(this IServiceCollection services, Dialog instance)
            where T : Dialog
        {
            // Register the Dialog.
            services.AddSingleton<T>((T)instance);
            return services;
        }

        /// <summary>
        /// Configures the service collection with a Dialog.
        /// </summary>
        /// <typeparam name="T">Type of the Dialog to register.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBotDialog<T>(this IServiceCollection services) 
            where T : Dialog
        {
            // Register the Dialog.
            services.AddSingleton<T>();
            return services;
        }

        /// <summary>
        /// Configures the service collection with a Bot.
        /// </summary>
        /// <typeparam name="T">Type of the Bot to register.</typeparam>
        /// <param name="services">The services collection.</param>
        /// <returns>The updated services collection.</returns>
        public static IServiceCollection UseBot<T>(this IServiceCollection services)
            where T : class, IBot
        {
            // Register the Bot.
            services.AddSingleton<T>();
            return services;
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
        }
    }
}
