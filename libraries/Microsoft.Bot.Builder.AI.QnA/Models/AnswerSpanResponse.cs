// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{    
    /// <summary>
    /// This class helps in identifying the precise answer within complete answer text.
    /// </summary>
    public class AnswerSpanResponse
    {
        /// <summary>
        /// Gets or sets the Precise Answer text.
        /// </summary>
        /// <value>
        /// The precise answer text relevant to the user query.
        /// </value>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the score of the Precise Answer.
        /// </summary>
        /// <value>
        /// The answer score pertaining to the quality of precise answer text.
        /// </value>
        [JsonProperty("score")]
        public float Score { get; set; }

        /// <summary>
        /// Gets or sets the  startIndex of the Precise Answer within the full answer text.
        /// </summary>
        /// <value>
        /// The starting index for the precise answer generated.
        /// </value>
        [JsonProperty("startIndex")]
        public int StartIndex { get; set; }

        /// <summary>
        /// Gets or sets the  endIndex of PreciseAnswer within the full answer text.
        /// </summary>
        /// <value>
        /// The end index for the precise answer generated.
        /// </value>
        [JsonProperty("endIndex")]
        public int EndIndex { get; set; }
    }
}
