// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.Adapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    /// <summary>
    /// Extension class for bot integration with ASP.NET Core 2.0 projects.
    /// </summary>
    /// <seealso cref="ApplicationBuilderExtensions"/>
    /// <seealso cref="BotAdapter"/>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures services for a <typeparamref name="TBot">specified bot type</typeparamref> to the <see cref="IServiceCollection" />.
        /// </summary>
        /// <typeparam name="TBot">A concrete type of <see cref="IBot"/ > that is to be registered and exposed to the Bot Framework.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A callback that can further be used to configure the bot.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddBot<TBot>(this IServiceCollection services, Action<BotFrameworkOptions> configureAction = null) where TBot : class, IBot
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configureAction != null)
            {
                services.Configure(configureAction);
            }

            services.AddTransient<IBot, TBot>();

            services.AddSingleton(sp =>
            {
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                var botFrameworkAdapter = new BotFrameworkAdapter(options.CredentialProvider, options.ConnectorClientRetryPolicy, options.HttpClient);

                foreach (var middleware in options.Middleware)
                {
                    botFrameworkAdapter.Use(middleware);
                }

                return botFrameworkAdapter;
            });

            return services;
        }
    }
}
