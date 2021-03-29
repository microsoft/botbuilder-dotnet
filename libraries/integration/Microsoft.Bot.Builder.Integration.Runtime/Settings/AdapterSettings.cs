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
        /// Gets the configuration key for <see cref="AdapterSettings"/>.
        /// </summary>
        /// <value>
        /// Configuration key for <see cref="AdapterSettings"/>.
        /// </value>
        public static string AdapterSettingsKey => $"{ConfigurationConstants.RuntimeSettingsKey}:adapters";

        /// <summary>
        /// Gets the default adapter route settings.
        /// </summary>
        /// <value>
        /// Default adapter route settings.
        /// </value>
        public static AdapterSettings CoreBotAdapterSettings => new AdapterSettings()
        {
            Enabled = true,
            Route = "messages",
            Name = nameof(CoreBotAdapter),
            Type = typeof(CoreBotAdapter).FullName
        };

        /// <summary>
        /// Gets or sets the name of the adapter.
        /// </summary>
        /// <value>
        /// Name of the adapter.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type name of the adapter.
        /// </summary>
        /// <value>
        /// Fully qualified name of the adapter.
        /// </value>
        public string Type { get; set; }

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
