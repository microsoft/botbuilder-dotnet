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
#pragma warning disable CA1819 // Properties should not return arrays (we can't change this without breaking binary compat)
        public QueryResult[] Answers { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays

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
