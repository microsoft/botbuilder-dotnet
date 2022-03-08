// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary>
    /// Gets or sets precise answer of query from Knowledge Base.
    /// </summary>
    [Serializable]
    internal class KnowledgeBaseAnswerSpan
    {
        /// <summary>
        /// Gets or sets the precise answer text.
        /// </summary>
        /// <value>
        /// The precise answer text.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the confidence in precise answer.
        /// </summary>
        /// <value>
        /// The confidence in precise answer.
        /// </value>
        [JsonProperty("confidenceScore")]
        public float ConfidenceScore { get; set; }

        /// <summary>
        /// Gets or sets the precise answer startIndex in long answer.
        /// </summary>
        /// <value>
        /// The precise answer startIndex in long answer.
        /// </value>
        [JsonProperty("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// Gets or sets the precise answer length in long answer.
        /// </summary>
        /// <value>
        /// The precise answer length in long answer.
        /// </value>
        [JsonProperty("length")]
        public int Length { get; set; }
    }
}
