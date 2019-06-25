// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.StreamingExtensions
{
    /// <summary>
    /// Used to register the 'front door' adapter that selects which adapter is needed to process an incoming HTTP request.
    /// GET requests asking for WebSocket upgrades are directed to a Bot Framework Protocol Version 3 with Streaming Extensions enabled adapter.
    /// POST requests are directed to a traditional Bot Framework adapter.
    /// </summary>
    public static partial class BotFrameworkServiceCollectionExtensions
    {
        /// <summary>
        /// Used in the Configuration section of Startup to register the 'front door' adapter for use with dependency injection into the bot controller.
        /// Throws <see cref="ArgumentNullException"/> if services is null.
        /// </summary>
        /// <param name="services">The current service collection, passed in here to enable use of the adapter.</param>
        /// <returns>The service collection, updated to include the new adapter.</returns>
        public static IServiceCollection UseWebSocketAdapter(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<WebSocketEnabledHttpAdapter>();
            return services;
        }
    }
}
