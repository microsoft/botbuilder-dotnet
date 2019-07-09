// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// Contains formatting options for presenting a list of choices.
    /// </summary>
    public class ChoiceFactoryOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceFactoryOptions"/> class.
        /// </summary>
        public ChoiceFactoryOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChoiceFactoryOptions"/> class.
        /// Refer to the code in teh ConfirmPrompt for an example of usage.
        /// </summary>
        /// <param name="inlineSeparator">The inline seperator value.</param>
        /// <param name="inlineOr">The inline or value.</param>
        /// <param name="inlineOrMore">The inline or more value.</param>
        /// <param name="includeNumbers">Flag indicating whether to include numbers as a choice.</param>
        public ChoiceFactoryOptions(string inlineSeparator, string inlineOr, string inlineOrMore, bool? includeNumbers)
        {
            InlineSeparator = inlineSeparator;
            InlineOr = inlineOr;
            InlineOrMore = inlineOrMore;
            IncludeNumbers = includeNumbers;
        }

        /// <summary>
        /// Gets or sets the character used to separate individual choices when there are more than 2 choices.
        /// The default value is `", "`. This is optional.
        /// </summary>
        /// <value>
        /// The character used to separate individual choices when there are more than 2 choices.
        /// </value>
        public string InlineSeparator { get; set; }

        /// <summary>
        /// Gets or sets the separator inserted between the choices when their are only 2 choices. The default
        /// value is `" or "`. This is optional.
        /// </summary>
        /// <value>
        /// The separator inserted between the choices when their are only 2 choices.
        /// </value>
        public string InlineOr { get; set; }

        /// <summary>
        /// Gets or sets the separator inserted between the last 2 choices when their are more than 2 choices.
        /// The default value is `", or "`. This is optional.
        /// </summary>
        /// <value>
        /// The separator inserted between the last 2 choices when their are more than 2 choices.
        /// </value>
        public string InlineOrMore { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an inline and list style choices will be prefixed with the index of the
        /// choice as in "1. choice". If <see langword="false"/>, the list style will use a bulleted list instead.The default value is <see langword="true"/>.
        /// </summary>
        /// <value>
        /// A <c>true</c>if an inline and list style choices will be prefixed with the index of the
        /// choice as in "1. choice"; otherwise a <c>false</c> and the list style will use a bulleted list instead.
        /// </value>
        public bool? IncludeNumbers { get; set; }
    }
}
