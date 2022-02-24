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
        /// Gets or sets the user id.
        /// </summary>
        /// <value>the user id.</value>
        [JsonProperty("userId")]
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the user question.
        /// </summary>
        /// <value>the user question.</value>
        [JsonProperty("userQuestion")]
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets the QnA Id.
        /// </summary>
        /// <value>the qnaMaker id.</value>
        [JsonProperty("qnaId")]
        public int QnaId { get; set; }
    }
}
