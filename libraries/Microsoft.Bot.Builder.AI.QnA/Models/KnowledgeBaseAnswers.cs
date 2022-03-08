// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary>
    /// Contains answers for a user query.
    /// </summary>
    internal class KnowledgeBaseAnswers
    {
        /// <summary>
        /// Gets the answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </summary>
        /// <value>
        /// The answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </value>
        [JsonProperty("answers")]
        public List<KnowledgeBaseAnswer> Answers { get; } = new List<KnowledgeBaseAnswer>();
    }
}
