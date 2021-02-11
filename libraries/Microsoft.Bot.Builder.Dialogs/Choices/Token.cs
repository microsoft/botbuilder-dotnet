// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Represents an individual token, such as a word in an input string.
    /// </summary>
    /// <seealso cref="Tokenizer"/>
    public class Token
    {
        /// <summary>
        /// Gets or sets the index of the first character of the token within the input.
        /// </summary>
        /// <value>The index of the first character of the token.</value>
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the index of the last character of the token within the input.
        /// </summary>
        /// <value>The index of the last character of the token.</value>
        public int End { get; set; }

        /// <summary>
        /// Gets or sets the original text of the token.
        /// </summary>
        /// <value>The original text of the token.</value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a normalized version of the token.
        /// </summary>
        /// <value>A normalized version of the token.</value>
        public string Normalized { get; set; }
    }
}
