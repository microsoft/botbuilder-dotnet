// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Contains options used to control how choices are recognized in a users utterance.
    /// </summary>
    public class FindValuesOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether only some of the tokens in a value need to exist to be considered
        /// a match. The default value is "false". This is optional.
        /// </summary>
        /// <value>
        /// A <c>true</c> if only some of the tokens in a value need to exist to be considered; otherwise <c>false</c>.
        /// </value>
        public bool AllowPartialMatches { get; set; }

        /// <summary>
        /// Gets or sets the locale/culture code of the utterance. The default is `en-US`. This is optional.
        /// </summary>
        /// <value>
        /// The locale/culture code of the utterance.
        /// </value>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or sets the maximum tokens allowed between two matched tokens in the utterance. So with
        /// a max distance of 2 the value "second last" would match the utterance "second from the last"
        /// but it wouldn't match "Wait a second. That's not the last one is it?".
        /// The default value is "2".
        /// </summary>
        /// <value>
        /// The maximum tokens allowed between two matched tokens in the utterance.
        /// </value>
        public int? MaxTokenDistance { get; set; }

        /// <summary>
        /// Gets or sets the tokenizer to use when parsing the utterance and values being recognized.
        /// </summary>
        /// <value>
        /// The tokenizer to use when parsing the utterance and values being recognized.
        /// </value>
        public TokenizerFunction Tokenizer { get; set; }
    }
}
