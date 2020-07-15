// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Active learning feedback records.
    /// </summary>
    public class FeedbackRecords
    {
        /// <summary>
        /// Gets or sets the list of feedback records.
        /// </summary>
        /// <value>
        /// List of feedback records.
        /// </value>
        [JsonProperty("feedbackRecords")]
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public FeedbackRecord[] Records { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
