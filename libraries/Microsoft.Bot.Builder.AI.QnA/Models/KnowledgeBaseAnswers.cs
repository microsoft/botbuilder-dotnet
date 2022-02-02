// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// Contains answers for a user query.
    /// </summary>
    public class KnowledgeBaseAnswers
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeBaseAnswers"/> class. Initializes a new instance of answers.
        /// </summary>
        /// <param name="answers">The answers for a user query,
        /// sorted in decreasing order of ranking score.</param>
        public KnowledgeBaseAnswers(List<KnowledgeBaseAnswer> answers)
        {
            Answers = answers;
        }

        /// <summary>
        /// Gets the answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </summary>
        /// <value>
        /// The answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </value>
        [JsonProperty("answers")]
        public List<KnowledgeBaseAnswer> Answers { get; }
    }
}
