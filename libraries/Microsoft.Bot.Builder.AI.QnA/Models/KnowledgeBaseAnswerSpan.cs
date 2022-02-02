// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Gets or sets AnswerSpan of the previous turn.
    /// </summary>
    public class KnowledgeBaseAnswerSpan
    {
        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        /// <value>
        /// The answer text.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the answer score.
        /// </summary>
        /// <value>
        /// The answer score.
        /// </value>
        [JsonProperty("confidenceScore")]
        public float ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the answer startIndex.
        /// </summary>
        /// <value>
        /// The answer startIndex.
        /// </value>
        [JsonProperty("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the answer endIndex.
        /// </summary>
        /// <value>
        /// The answer endIndex.
        /// </value>
        [JsonProperty("length")]
        public int Length { get; set; }
    }
}
