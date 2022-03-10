// Copyright (c) Microsoft Corporation.
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
        /// Gets any extra properties to include in the results.
        /// </summary>
        /// <value>
        /// Any extra properties to include in the results.
        /// </value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; private set; } = new Dictionary<string, object>();
    }
}
