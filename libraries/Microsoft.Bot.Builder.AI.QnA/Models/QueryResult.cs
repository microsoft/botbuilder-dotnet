﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Represents an individual result from a knowledge base query.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// Gets the list of questions indexed in the QnA Service for the given answer.
        /// </summary>
        /// <value>
        /// The list of questions indexed in the QnA Service for the given answer.
        /// </value>
        [JsonProperty("questions")]
        public Collection<string> Questions { get; private set; } = new Collection<string>();

        /// <summary>
        /// Gets or sets the answer text.
        /// </summary>
        /// <value>
        /// The answer text.
        /// </value>
        [JsonProperty("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// Gets or sets the answer's score, from 0.0 (least confidence) to
        /// 1.0 (greatest confidence).
        /// </summary>
        /// <value>
        /// The answer's score, from 0.0 (least confidence) to
        /// 1.0 (greatest confidence).
        /// </value>
        [JsonProperty("score")]
        public float Score { get; set; }

        /// <summary>
        /// Gets metadata that is associated with the answer.
        /// </summary>
        /// <value>
        /// Metadata that is associated with the answer.
        /// </value>
        [JsonProperty(PropertyName = "metadata")]
        public Collection<Metadata> Metadata { get; private set; } = new Collection<Metadata>();

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
        [JsonProperty(PropertyName = "context")]
        public QnAResponseContext Context { get; set; }
    }
}
