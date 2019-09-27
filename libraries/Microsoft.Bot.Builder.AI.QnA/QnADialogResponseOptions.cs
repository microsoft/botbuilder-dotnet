// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnA dialog response options class.
    /// </summary>
    public class QnADialogResponseOptions
    {
        /// <summary>
        /// Gets or sets get or set for No answer.
        /// </summary>
        public ITemplate<Activity> NoAnswer { get; set; }

        /// <summary>
        /// Gets or sets get or set for Active learning card title.
        /// </summary>
        public string ActiveLearningCardTitle { get; set; }

        /// <summary>
        /// Gets or sets get or set for Card no match text.
        /// </summary>
        public string CardNoMatchText { get; set; }

        /// <summary>
        /// Gets or sets get or set for Card no match response.
        /// </summary>
        public ITemplate<Activity> CardNoMatchResponse { get; set; }
    }
}
