// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.AI.QnA
{
    /// <summary>
    /// QnA dialog response options class.
    /// </summary>
    public class QnADialogResponseOptions
    {
        /// <summary>
        /// Gets or sets get or set for Active learning card title.
        /// </summary>
        /// <value>
        /// Get or set for Active learning card title.
        /// </value>
        public string ActiveLearningCardTitle { get; set; }

        /// <summary>
        /// Gets or sets get or set for Card no match text.
        /// </summary>
        /// <value>
        /// Get or set for Card no match text.
        /// </value>
        public string CardNoMatchText { get; set; }

        /// <summary>
        /// Gets or sets get or set for No answer.
        /// </summary>
        /// <value>
        /// Get or set for No answer.
        /// </value>
        public Activity NoAnswer { get; set; }

        /// <summary>
        /// Gets or sets get or set for Card no match response.
        /// </summary>
        /// <value>
        /// Get or set for Card no match response.
        /// </value>
        public Activity CardNoMatchResponse { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Precise Answer is to be displayed or the source text also
        /// chosen to be displayed to the user.
        /// </summary>
        /// <value>
        /// Get or set whether to display Precise Answer Only or source text along with Precise Answer.
        /// </value>
        public bool DisplayPreciseAnswerOnly { get; set; }
    }
}
