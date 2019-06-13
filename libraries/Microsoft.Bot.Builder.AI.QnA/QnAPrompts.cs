// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// The context of the question that is returned from the QnA Maker API.  Used to return related follow-up prompts.
    /// Allows a bot to do qna "multi-turn" conversations from one question to another.  So the user can journey through
    /// a series of filtered/refined questions to get to a final answer.  This requires the QnA knowledge base to have been
    /// configured with follow-up prompts to individual questions.
    /// </summary>
    public class QnAPrompts
    {
        /// <summary>
        /// Gets or sets the index of the prompt, used for ordering.
        /// </summary>
        /// <value>
        /// The display order.
        /// </value>
        [JsonProperty("displayOrder")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the QnADTO returned from the API - is always null.
        /// </summary>
        /// <value>
        /// The QnA DTO.
        /// </value>
        [JsonProperty("qna")]
        public object Qna { get; set; }

        /// <summary>
        /// Gets or sets the unique QnA Id of the prompt.
        /// </summary>
        /// <value>
        /// The QnA id.
        /// </value>
        [JsonProperty("qnaId")]
        public int QnaId { get; set; }

        /// <summary>
        /// Gets or sets the text to display for the follow-up prompt.
        /// </summary>
        /// <value>
        /// The display text of the prompt.
        /// </value>
        [JsonProperty("displayText")]
        public string DisplayText { get; set; }
    }
}
