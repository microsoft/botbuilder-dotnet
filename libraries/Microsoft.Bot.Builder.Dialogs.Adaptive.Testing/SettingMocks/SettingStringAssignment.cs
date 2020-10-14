// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.SettingMocks
{
    /// <summary>
    /// Setting String Assignment (used in SettingStringMock).
    /// </summary>
    public class SettingStringAssignment
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
