// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Runtime.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Integration.Runtime.Plugins
{
    /// <summary>
    /// Provides a standard implementation of <see cref="IBotPluginLoadContext"/>.
    /// </summary>
    internal class BotPluginLoadContext : IBotPluginLoadContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotPluginLoadContext"/> class.
        /// </summary>
        /// <param name="configuration">Application settings used for configuring the plugin.</param>
        public BotPluginLoadContext(IConfiguration configuration)
        {
            this.Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets the application settings used for configuring the plugin.
        /// </summary>
        /// <value>
        /// Application settings used for configuring the plugin.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the collection of services to be registered and made available to the runtime
        /// via dependency injection.
        /// </summary>
        /// <value>
        /// The collection of services to be registered and made available to the runtime via
        /// dependency injection.
        /// </value>
        public IServiceCollection Services { get; } = new ServiceCollection();
    }
}
