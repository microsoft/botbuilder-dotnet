// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary>
    /// Gets or sets short answer of query from Knowledge Base.
    /// </summary>
    public class KnowledgeBaseAnswerSpan
    {
        /// <summary>
        /// Gets or sets the short answer text.
        /// </summary>
        /// <value>
        /// The short 
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the confidence in short answer.
        /// </summary>
        /// <value>
        /// The confidence in short answer.
        /// </value>
        [JsonProperty("confidenceScore")]
        public float ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the short answer startIndex in long answer.
        /// </summary>
        /// <value>
        /// The short answer startIndex in long answer.
        /// </value>
        [JsonProperty("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the short answer length in long answer.
        /// </summary>
        /// <value>
        /// The short answer length in long answer.
        /// </value>
        [JsonProperty("length")]
        public int Length { get; set; }
    }
}
