// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    public class AnswerSpanResponse
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
        [JsonProperty("score")]
        public float Score { get; set; }

        /// <summary>
        /// Gets or sets the answer startIndex.
        /// </summary>
        /// <value>
        /// The answer startIndex.
        /// </value>
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the answer endIndex.
        /// </summary>
        /// <value>
        /// The answer endIndex.
        /// </value>
        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
    }
}
