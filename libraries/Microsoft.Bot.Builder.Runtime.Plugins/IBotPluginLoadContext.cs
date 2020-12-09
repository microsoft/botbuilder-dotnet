// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Builder.Runtime.Plugins
{
    /// <summary>
    /// Exposes utilities required in the context of loading a plugin into the bot runtime
    /// via <see cref="IBotPlugin.Load"/>.
    /// </summary>
    public interface IBotPluginLoadContext
    {
        /// <summary>
        /// Gets the application settings used for configuring the plugin.
        /// </summary>
        /// <value>
        /// Application settings used for configuring the plugin.
        /// </value>
        IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the collection of services to be registered and made available to the runtime
        /// via dependency injection.
        /// </summary>
        /// <value>
        /// The collection of services to be registered and made available to the runtime via
        /// dependency injection.
        /// </value>
        IServiceCollection Services { get; }
    }
}
