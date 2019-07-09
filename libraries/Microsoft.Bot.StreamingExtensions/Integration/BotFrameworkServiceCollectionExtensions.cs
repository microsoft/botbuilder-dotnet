// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.StreamingExtensions.Integration
{
    /// <summary>
    /// An extention to the Service Collection to allow registration of the Bot Framework Protocol Version 3 with Streaming Extensions adapter for use with named pipe connections.
    /// </summary>
    public static partial class BotFrameworkServiceCollectionExtensions
    {
        /// <summary>
        /// Used in the Configuration section of Startup to register the adapter for use with named pipe connections.
        /// Throws <see cref="ArgumentNullException"/> if services is null.
        /// </summary>
        /// <param name="services">The current service collection, passed in here to enable use of the adapter.</param>
        /// <returns>The service collection, updated to include the new adapter.</returns>
        public static IServiceCollection AddNamedPipeConnector(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var connector = new NamedPipeConnector();

            services.AddSingleton(connector);

            return services;
        }
    }
}
