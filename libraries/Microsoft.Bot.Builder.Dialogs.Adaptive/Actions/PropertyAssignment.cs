// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Property Assignment (used in SetProperty and SetProperties actions).
    /// </summary>
    public class PropertyAssignment
    {
        /// <summary>
        /// Gets or sets the property path.
        /// </summary>
        /// <value>A property path expression.</value>
        [JsonProperty("property")]
        public StringExpression Property { get; set; }

        /// <summary>
        /// Gets or sets the value to set.
        /// </summary>
        /// <value>Value expression.</value>
        [JsonProperty("value")]
        public ValueExpression Value { get; set; }
    }
}
