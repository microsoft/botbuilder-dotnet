// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.PropertyMocks
{
    /// <summary>
    /// Property Assignment (used in PropertiesMock).
    /// </summary>
    public class PropertyAssignment
    {
        /// <summary>
        /// Gets or sets the property path.
        /// </summary>
        /// <value>A property path.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the value to set.
        /// </summary>
        /// <value>The value. In settings, it could only be string.</value>
        [JsonProperty("value")]
        public object Value { get; set; }
    }
}
