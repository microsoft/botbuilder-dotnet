// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.PropertyMocks
{
    /// <summary>
    /// Property Assignment (used in SettingsPropertiesMock).
    /// </summary>
    public class SettingsPropertyAssignment
    {
        /// <summary>
        /// Gets or sets the property path.
        /// </summary>
        /// <value>A property path without settings.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the value to set.
        /// </summary>
        /// <value>Value string.</value>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
