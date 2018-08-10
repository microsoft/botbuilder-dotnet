// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.Translation
{
    /// <summary>
    /// Translated document is the data object holding all information of the translator module output on an input string.
    /// </summary>
    public class TranslatedDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TranslatedDocument"/> class  using only source message.
        /// </summary>
        /// <param name="sourceMessage">Source message.</param>
        public TranslatedDocument(string sourceMessage)
        {
            if (string.IsNullOrWhiteSpace(sourceMessage))
            {
                throw new ArgumentNullException(nameof(sourceMessage));
            }

            this.SourceMessage = sourceMessage;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslatedDocument"/> class using source message and target/translated message.
        /// </summary>
        /// <param name="sourceMessage">Source message.</param>
        /// <param name="targetMessage">Target/translated message.</param>
        public TranslatedDocument(string sourceMessage, string targetMessage)
        {
            if (string.IsNullOrWhiteSpace(sourceMessage))
            {
                throw new ArgumentNullException(nameof(sourceMessage));
            }

            if (string.IsNullOrWhiteSpace(targetMessage))
            {
                throw new ArgumentNullException(nameof(targetMessage));
            }

            this.SourceMessage = sourceMessage;
            this.TargetMessage = targetMessage;
        }

        public string SourceMessage { get; set; }

        public string TargetMessage { get; set; }

        public string RawAlignment { get; set; }

        public Dictionary<int, int> IndexedAlignment { get; set; }

        public string[] SourceTokens { get; set; }

        public string[] TranslatedTokens { get; set; }

        public HashSet<string> LiteranlNoTranslatePhrases { get; set; }
    }
}
