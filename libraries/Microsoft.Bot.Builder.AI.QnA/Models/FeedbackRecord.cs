// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Active learning feedback record.
    /// </summary>
    public class FeedbackRecord
    {
        /// <summary>
        /// Gets or sets user id.
        /// </summary>
        /// <value>
        /// User id.
        /// </value>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets user question.
        /// </summary>
        /// <value>
        /// User question.
        /// </value>
        [JsonProperty("userQuestion")]
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets qnA Id.
        /// </summary>
        /// <value>
        /// QnA Id.
        /// </value>
        [JsonProperty("qnaId")]
        public int QnaId { get; set; }
    }
}
