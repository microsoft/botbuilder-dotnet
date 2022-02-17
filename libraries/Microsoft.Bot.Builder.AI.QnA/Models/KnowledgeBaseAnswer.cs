// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary>
    /// Represents an individual result from a knowledge base query.
    /// </summary>
    public class KnowledgeBaseAnswer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseAnswer"/> class.
        /// </summary>
        /// <param name="metadata">Metadata associated with the answer, useful to categorize or filter question answers.</param>
        /// <param name="questions">The list of questions indexed in the QnA Service for the given answer.</param>
        public KnowledgeBaseAnswer(Dictionary<string, string> metadata, List<string> questions)
        {
            Metadata = metadata;
            Questions = questions;
        }

        /// <summary>
        /// Gets the list of questions indexed in the QnA Service for the given answer.
        /// </summary>
        /// <value>
        /// The list of questions indexed in the QnA Service for the given answer.
        /// </value>
        [JsonProperty("questions")]
        public List<string> Questions { get; }

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        /// <value>
        /// The answer text.
        /// </value>
        [JsonProperty("answer")]
        public string Answer { get; set; }

        /// <summary> Gets metadata associated with the answer, useful to categorize or filter question answers. </summary>
        /// <value>  Metadata associated with the answer, useful to categorize or filter question answers.</value>
        [JsonProperty(PropertyName = "metadata")]
        public Dictionary<string, string> Metadata { get; }

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
