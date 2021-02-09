// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Integration.Runtime.Plugins
{
    /// <summary>
    /// Provides an interface for retrieving a collection of bot plugins from a given source.
    /// </summary>
    internal interface IBotPluginEnumerator
    {
        /// <summary>
        /// Get available bot plugins.
        /// </summary>
        /// <param name="pluginName">Bot plugin identifier used to retrieve applicable plugins.</param>
        /// <returns>A collection of available bot plugins for the specified plugin name.</returns>
        IEnumerable<IBotPlugin> GetPlugins(string pluginName);
    }
}
