// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Prompt Object.
    /// </summary>
    public class QnaMakerPrompt
    {
        private const int DefaultDisplayOrder = 0;

        /// <summary>
        /// Gets or sets displayOrder - index of the prompt - used in ordering of the prompts.
        /// </summary>
        /// <value>Display order.</value>
        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; } = DefaultDisplayOrder;

        /// <summary>
        /// Gets or sets qna id corresponding to the prompt - if QnaId is present, QnADTO object is ignored.
        /// </summary>
        /// <value>QnA Id.</value>
        [JsonProperty("qnaId")]
        public int QnaId { get; set; }

        /// <summary>
        /// Gets or sets displayText - Text displayed to represent a follow up question prompt.
        /// </summary>
        /// <value>Display test.</value>
        [JsonProperty("displayText")]
        public string DisplayText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the QnADTO returned from the API.
        /// </summary>
        /// <value>
        /// The QnA DTO.
        /// </value>
        [JsonProperty("qna")]
        public object Qna { get; set; }
    }
}
