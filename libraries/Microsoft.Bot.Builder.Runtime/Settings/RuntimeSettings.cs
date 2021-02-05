// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Runtime.Plugins;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    /// <summary>
    /// Settings for the bot runtime.
    /// </summary>
    internal class RuntimeSettings
    {
        /// <summary>
        /// Gets or sets the settings for runtime features.
        /// </summary>
        /// <value>
        /// The settings for runtime features.
        /// </value>
        public FeatureSettings Features { get; set; }

        /// <summary>
        /// Gets or sets the settings for runtime resources, such as adapters and storage.
        /// </summary>
        /// <value>
        /// The settings for runtime resources, such as adapters and storage.
        /// </value>
        public ResourcesSettings Resources { get; set; }

        /// <summary>
        /// Gets or sets the telemetry settings for the runtime.
        /// </summary>
        /// <value>
        /// The telemetry settings for the runtime.
        /// </value>
        public TelemetrySettings Telemetry { get; set; }

        /// <summary>
        /// Gets or sets the skill settings for the runtime.
        /// </summary>
        /// <value>
        /// The skill settings for the runtime.
        /// </value>
        public SkillSettings Skills { get; set; }

        /// <summary>
        /// Gets or sets the list of plugins registered for the runtime.
        /// </summary>
        /// <value>
        /// The list of plugins registered for the runtime.
        /// </value>
        public IList<BotPluginDefinition> Plugins { get; set; } = new List<BotPluginDefinition>();
    }
}
