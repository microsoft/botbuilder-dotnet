// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{    
    /// <summary>
    /// Represents Precise Answer details, these are generated when Precise Answer generation choice is enabled.
    /// </summary>
    public class AnswerSpanResponse
    {
        /// <summary>
        /// Gets or sets the Precise Answer text.
        /// </summary>
        /// <value>
        /// The answer text.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the score of Precise Answer.
        /// </summary>
        /// <value>
        /// The answer score.
        /// </value>
        [JsonProperty("score")]
        public float Score { get; set; }

        /// <summary>
        /// Gets or sets the  startIndex of Precise Answer in Source Answer Text.
        /// </summary>
        /// <value>
        /// The answer startIndex.
        /// </value>
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the  endIndex of PreciseAnswer in Source Answer Text.
        /// </summary>
        /// <value>
        /// The answer endIndex.
        /// </value>
        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
    }
}
