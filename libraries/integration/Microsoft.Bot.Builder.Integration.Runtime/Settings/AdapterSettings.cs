// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Integration.Runtime.Settings
{
    /// <summary>
    /// Settings for configurable bot adapters.
    /// </summary>
    public class AdapterSettings
    {
        /// <summary>
        /// Gets or sets the type name of the adapter.
        /// </summary>
        /// <value>
        /// Fully qualified name of the adapter.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the route in which to expose the adapter exposed over http.
        /// </summary>
        /// <value>
        /// Route in which to expose the adapter exposed over http.
        /// </value>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the adapter is enabled.
        /// </summary>
        /// <value>
        /// Value indicating whether the adapter is enabled.
        /// </value>
        public bool Enabled { get; set; } = true;
    }
}
