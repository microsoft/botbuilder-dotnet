// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// A set of extension methods for <see cref="IServiceCollection"/> which provide support for hosting bots with .NET Core.
    /// </summary>
    /// <seealso cref="ApplicationBuilderExtensions"/>
    /// <seealso cref="IAdapterIntegration"/>
    /// <seealso cref="IBot"/>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and optionally configures a <typeparamref name="TBot">specified bot type</typeparamref> to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TBot">A concrete type of <see cref="IBot"/> that is to be registered and exposed to the Bot Framework.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A optional callback that, if provided, will be invoked to further configure of the bot.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <remarks>
        ///     The <typeparamref name="TBot"/> will be registered as <see cref="ServiceLifetime.Transient">transient</see> and be instantiated on each turn.
        /// </remarks>
        /// <seealso cref="IBot"/>
        public static IServiceCollection AddBot<TBot>(this IServiceCollection services, Action<BotFrameworkOptions> configureAction = null)
            where TBot : class, IBot
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureAction != null)
            {
                services.Configure(configureAction);
            }

            return services
                .TryAddBotFrameworkAdapterIntegration()
                .AddTransient<IBot, TBot>();
        }

        /// <summary>
        /// Adds and optionally configures a singleton <paramref name="bot">bot</paramref> instance to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TBot">A concrete type of <see cref="IBot"/> that is to be registered and exposed to the Bot Framework.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="bot">The instance of the bot that will be registered as a <see cref="ServiceLifetime.Singleton"/>.</param>
        /// <param name="configureAction">A optional callback that, if provided, will be invoked to further configure of the bot.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <seealso cref="IBot"/>
        public static IServiceCollection AddBot<TBot>(this IServiceCollection services, TBot bot, Action<BotFrameworkOptions> configureAction = null)
        where TBot : class, IBot
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (bot == null)
            {
                throw new ArgumentNullException(nameof(bot));
            }

            if (configureAction != null)
            {
                services.Configure(configureAction);
            }

            return services
                .TryAddBotFrameworkAdapterIntegration()
                .AddSingleton<IBot>(bot);
        }

        /// <summary>
        /// Adds and optionally configures a <typeparamref name="TBot">specified bot type</typeparamref> to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TBot">A concrete type of <see cref="IBot"/> that is to be registered and exposed to the Bot Framework.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="botFactory">A factory method that will supply an instance of the <typeparamref name="TBot"/> when invoked.</param>
        /// <param name="configureAction">A optional callback that, if provided, will be invoked to further configure of the bot.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <remarks>
        ///     The <paramref name="botFactory">factory</paramref> will be registered as <see cref="ServiceLifetime.Transient">transient</see>
        ///     and be invoked on each turn.
        /// </remarks>
        /// <seealso cref="IBot"/>
        public static IServiceCollection AddBot<TBot>(this IServiceCollection services, Func<IServiceProvider, TBot> botFactory, Action<BotFrameworkOptions> configureAction = null)
            where TBot : class, IBot
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (botFactory == null)
            {
                throw new ArgumentNullException(nameof(botFactory));
            }

            if (configureAction != null)
            {
                services.Configure(configureAction);
            }

            return services
                .TryAddBotFrameworkAdapterIntegration()
                .AddSingleton<IBot>(botFactory);
        }

        /// <summary>
        /// Adds the <see cref="BotFrameworkAdapter"/> as the <see cref="IAdapterIntegration"/> which will be used by the integration layer
        /// for processing bot requests.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A optional callback that, if provided, will be invoked to further configure the integration.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        /// <remarks>
        ///     The <see cref="BotFrameworkAdapter"/> will be registered as a <see cref="ServiceLifetime.Singleton">singleton</see>.
        ///
        ///     NOTE: Calling any of the <c>AddBot</c> overloads those will attempt to implicitly register this for you if there is no
        ///     explicit <see cref="IAdapterIntegration"/> already registered in the <paramref name="services"/> collection.
        /// </remarks>
        /// <seealso cref="AddBot{TBot}(IServiceCollection, Action{BotFrameworkOptions})"/>
        /// <seealso cref="BotFrameworkAdapter"/>
        /// <seealso cref="IAdapterIntegration"/>
        public static IServiceCollection AddBotFrameworkAdapterIntegration(this IServiceCollection services, Action<BotFrameworkOptions> configureAction = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureAction != null)
            {
                services.Configure(configureAction);
            }

            return services.AddSingleton<IAdapterIntegration>(BotFrameworkAdapterSingletonFactory);
        }

        private static IServiceCollection TryAddBotFrameworkAdapterIntegration(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IAdapterIntegration>(BotFrameworkAdapterSingletonFactory);

            return services;
        }

        private static BotFrameworkAdapter BotFrameworkAdapterSingletonFactory(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<IAdapterIntegration>>();

            var botFrameworkAdapter = new BotFrameworkAdapter(
                            options.CredentialProvider,
                            options.AuthenticationConfiguration,
                            options.ChannelProvider,
                            options.ConnectorClientRetryPolicy,
                            options.HttpClient,
                            null,
                            logger)
            {
                OnTurnError = options.OnTurnError,
            };

            foreach (var middleware in options.Middleware)
            {
                botFrameworkAdapter.Use(middleware);
            }

            return botFrameworkAdapter;
        }
    }
}
