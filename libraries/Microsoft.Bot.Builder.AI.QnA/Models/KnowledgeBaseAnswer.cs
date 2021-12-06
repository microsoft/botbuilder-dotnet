﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Represents an individual result from a knowledge base query.
    /// </summary>
    public class KnowledgeBaseAnswer
    {
        /// <summary>
        /// Gets or sets the list of questions indexed in the QnA Service for the given answer.
        /// </summary>
        /// <value>
        /// The list of questions indexed in the QnA Service for the given answer.
        /// </value>
        [JsonProperty("questions")]
#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> Questions { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        /// <value>
        /// The answer text.
        /// </value>
        [JsonProperty("answer")]
        public string Answer { get; set; }

        /// <summary> Gets or sets metadata associated with the answer, useful to categorize or filter question answers. </summary>
        /// <value>
        ///  A value of Metadata associated with the answer, useful to categorize or filter question answers.
        /// </value>
        [JsonProperty(PropertyName = "metadata")]
#pragma warning disable CA2227 // Collection properties should be read only
        public Dictionary<string, string> Metadata { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets the answer's score, from 0.0 (least confidence) to
        /// 1.0 (greatest confidence).
        /// </summary>
        /// <value>
        /// The answer's score, from 0.0 (least confidence) to
        /// 1.0 (greatest confidence).
        /// </value>
        [JsonProperty("confidenceScore")]
        public double ConfidenceScore { get; set; }

       /// <summary>
        /// Gets or sets the source from which the QnA was extracted.
        /// </summary>
        /// <value>
        /// The source from which the QnA was extracted.
        /// </value>
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the index of the answer in the knowledge base. V3 uses
        /// 'qnaId', V4 uses 'id'.
        /// </summary>
        /// <value>
        /// The index of the answer in the knowledge base. V3 uses
        /// 'qnaId', V4 uses 'id'.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets context for multi-turn responses.
        /// </summary>
        /// <value>
        /// The context from which the QnA was extracted.
        /// </value>
        [JsonProperty(PropertyName = "dialog")]
        public QnAResponseContext Dialog { get; set; }

        /// <summary>
        /// Gets or sets AnswerSpan of the previous turn.
        /// </summary>
        /// <value>
        /// The answerspan value.
        /// </value>
        [JsonProperty("answerSpan")]
        public KnowledgeBaseAnswerSpan AnswerSpan { get; set; }
    }
}
