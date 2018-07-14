// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.Luis
{
    /// <summary>
    /// Strongly typed informtion about an intent.
    /// </summary>
    public class IntentData
    {
        /// <summary>
        /// Gets or sets the confidence in intent classification.
        /// </summary>
        /// <value>
        /// Confidence in intent classification.
        /// </value>
        [JsonProperty("score")]
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets any extra properties.
        /// </summary>
        /// <value>
        /// Any extra properties.
        /// </value>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; }
    }
}
