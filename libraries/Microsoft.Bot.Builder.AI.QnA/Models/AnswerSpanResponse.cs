// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary>
    /// Stores short answer of query from Knowledge Base.
    /// </summary>
    public class AnswerSpanResponse
    {
        /// <summary>
        /// Gets or sets the short answer text.
        /// </summary>
        /// <value>
        /// The short answer text.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the confidence in short answer.
        /// </summary>
        /// <value>
        /// The confidence in short answer.
        /// </value>
        [JsonProperty("score")]
        public float Score { get; set; }

        /// <summary>
        /// Gets or sets the short answer startIndex in long answer.
        /// </summary>
        /// <value>
        /// The short answer startIndex in long answer.
        /// </value>
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the short answer endIndex in long answer.
        /// </summary>
        /// <value>
        /// The short answer endIndex in long answer.
        /// </value>
        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
    }
}
