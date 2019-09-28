// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Contains answers for a user query.
    /// </summary>
    public class QueryResults
    {
        /// <summary>
        /// Gets or sets the answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </summary>
        /// <value>
        /// The answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </value>
        [JsonProperty("answers")]
        public QueryResult[] Answers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or set for the active learning enable flag.
        /// </summary>
        /// <value>
        /// The active learning enable flag.
        /// </value>
        [JsonProperty("activeLearningEnabled")]
        public bool ActiveLearningEnabled { get; set; }
    }
}
