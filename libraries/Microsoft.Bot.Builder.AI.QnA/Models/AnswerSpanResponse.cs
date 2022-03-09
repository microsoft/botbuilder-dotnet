// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary>
    /// Stores precise answer of query from Knowledge Base.
    /// </summary>
    public class AnswerSpanResponse
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
        [JsonProperty("score")]
        public float Score { get; set; }

        /// <summary>
        /// Gets or sets the precise answer startIndex in long answer.
        /// </summary>
        /// <value>
        /// The precise answer startIndex in long answer.
        /// </value>
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the precise answer endIndex in long answer.
        /// </summary>
        /// <value>
        /// The precise answer endIndex in long answer.
        /// </value>
        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
    }
}
