// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Runtime.Builders.Handlers;
using Microsoft.Bot.Builder.Runtime.Builders.Middleware;
using Microsoft.Bot.Builder.Runtime.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Runtime.Providers.Adapter
{
    /// <summary>
    /// Defines an implementation of <see cref="IAdapterProvider"/> that registers
    /// <see cref="CoreBotAdapter"/> with the application's service collection.
    /// </summary>
    [JsonObject]
    public class BotCoreAdapterProvider : IAdapterProvider
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BotCoreAdapter";

        /// <summary>
        /// Gets the collection of <see cref="IMiddlewareBuilder"/> instances used to construct the
        /// middleware pipeline for the adapter.
        /// </summary>
        /// <value>
        /// The collection of <see cref="IMiddlewareBuilder"/> instances used to construct the
        /// middleware pipeline for the adapter.
        /// </value>
        [JsonProperty("middleware")]
        public IList<IMiddlewareBuilder> Middleware { get; } = new List<IMiddlewareBuilder>();

        [JsonProperty("onTurnError")]
        public IOnTurnErrorHandlerBuilder OnTurnError { get; set; } = new OnTurnErrorHandlerBuilder();

        /// <summary>
        /// Register services with the application's service collection.
        /// </summary>
        /// <param name="services">The application's collection of registered services.</param>
        /// <param name="configuration">Application configuration.</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            services.Configure<CoreBotAdapterOptions>(o =>
            {
                foreach (IMiddlewareBuilder middleware in this.Middleware)
                {
                    o.Middleware.Add(middleware);
                }

                o.OnTurnError = this.OnTurnError;
            });

            services.AddSingleton<IBotFrameworkHttpAdapter, CoreBotAdapter>();
            services.AddSingleton<BotAdapter>(
                sp => (BotFrameworkHttpAdapter)sp.GetService<IBotFrameworkHttpAdapter>());
        }
    }
}
