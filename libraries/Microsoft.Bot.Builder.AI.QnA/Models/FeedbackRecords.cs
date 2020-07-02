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
        public FeedbackRecord[] Records { get; set; }
    }
}
