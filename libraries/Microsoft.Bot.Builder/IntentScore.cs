// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Score plus any extra information about an intent.
    /// </summary>
    public class IntentScore
    {
        /// <summary>
        /// Gets or sets confidence in an intent.
        /// </summary>
        /// <value>
        /// Confidence in an intent.
        /// </value>
        [JsonProperty("score")]
        public double? Score { get; set; }

        /// <summary>
        /// Gets or sets any extra properties to include in the results.
        /// </summary>
        /// <value>
        /// Any extra properties to include in the results.
        /// </value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
#pragma warning disable CA2227 // Collection properties should be read only  (we can't change this without breaking binary compat)
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
