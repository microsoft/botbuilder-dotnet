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
        /// User id.
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// User question.
        /// </summary>
        [JsonProperty("userQuestion")]
        public string UserQuestion { get; set; }

        /// <summary>
        /// QnA Id.
        /// </summary>
        [JsonProperty("qnaId")]
        public int QnaId { get; set; }
    }
}
