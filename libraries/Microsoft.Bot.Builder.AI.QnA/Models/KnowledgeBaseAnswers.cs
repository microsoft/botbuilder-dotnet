// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Contains answers for a user query.
    /// </summary>
    public class KnowledgeBaseAnswers
    {
        /// <summary>
        /// Gets or sets the answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </summary>
        /// <value>
        /// The answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </value>
        [JsonProperty("answers")]
        public List<KnowledgeBaseAnswer> Answers { get; set; }
    }
}
