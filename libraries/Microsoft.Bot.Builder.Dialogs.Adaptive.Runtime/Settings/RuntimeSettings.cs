// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Component;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings
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
        public FeatureSettings Features { get; set; } = new FeatureSettings();

        /// <summary>
        /// Gets or sets the telemetry settings for the runtime.
        /// </summary>
        /// <value>
        /// The telemetry settings for the runtime.
        /// </value>
        public TelemetrySettings Telemetry { get; set; } = new TelemetrySettings();

        /// <summary>
        /// Gets or sets the skill settings for the runtime.
        /// </summary>
        /// <value>
        /// The skill settings for the runtime.
        /// </value>
        public SkillSettings Skills { get; set; } = new SkillSettings();

        /// <summary>
        /// Gets or sets the list of components registered for the runtime.
        /// </summary>
        /// <value>
        /// The list of components registered for the runtime.
        /// </value>
        public IList<BotComponentDefinition> Components { get; set; } = new List<BotComponentDefinition>();

        /// <summary>
        /// Gets or sets the list of adapters to expose in the runtime.
        /// </summary>
        /// <value>
        /// The list of adapters to expose in the runtime.
        /// </value>
        public IList<AdapterSettings> Adapters { get; set; } = new List<AdapterSettings>();

        /// <summary>
        /// Gets or sets the name of the storage to use.
        /// </summary>
        /// <value>
        /// The name of the storage to use.
        /// </value>
        public string Storage { get; set; }
    }
}
