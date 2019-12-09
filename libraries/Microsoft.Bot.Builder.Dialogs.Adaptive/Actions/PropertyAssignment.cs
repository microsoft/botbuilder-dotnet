using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Property Assignment.
    /// </summary>
    public class PropertyAssignment
    {
        /// <summary>
        /// Gets or sets the property path.
        /// </summary>
        /// <value>A property path expression.</value>
        [JsonProperty("property")]
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets the value to set.
        /// </summary>
        /// <value>Value expression.</value>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
