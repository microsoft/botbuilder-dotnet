// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    /// <summary>
    /// Culture model used in Choice and Confirm Prompts.
    /// </summary>
    public class PromptCultureModel
    {
        /// <summary>
        /// Gets or Sets Culture Model's Locale.
        /// </summary>
        /// <value>
        /// Example: "en-US".
        /// </value>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or Sets Culture Model's InlineSeparator.
        /// </summary>
        /// <value>
        /// Example: ", ".
        /// </value>
        public string Separator { get; set; }

        /// <summary>
        /// Gets or Sets Culture Model's InlineOr.
        /// </summary>
        /// <value>
        /// Example: " or ".
        /// </value>
        public string InlineOr { get; set; }

        /// <summary>
        /// Gets or Sets Culture Model's InlineOrMore.
        /// </summary>
        /// <value>
        /// Example: ", or ".
        /// </value>
        public string InlineOrMore { get; set; }

        /// <summary>
        /// Gets or Sets Equivalent of "Yes" in Culture Model's Language.
        /// </summary>
        /// <value>
        /// Example: "Yes".
        /// </value>
        public string YesInLanguage { get; set; }

        /// <summary>
        /// Gets or Sets Equivalent of "No" in Culture Model's Language.
        /// </summary>
        /// <value>
        /// Example: "No".
        /// </value>
        public string NoInLanguage { get; set; }
    }
}
