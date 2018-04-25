// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.LUIS
{
    /// <summary>
    /// Strongly typed informtion about an intent.
    /// </summary>
    public class IntentData
    {
        /// <summary>
        /// Optional confidence in intent classification.
        /// </summary>
        [JsonProperty("score")]
        public double? Score { get; set; }
    }
}
