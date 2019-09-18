namespace Microsoft.Bot.Builder.Dialogs.Prompts
{
    /// <summary>
    /// Culture model used in Choice and Confirm Prompts.
    /// </summary>
    public class IPromptCultureModel
    {
        /// <summary>
        /// Gets or Sets Culture Model's Locale.
        /// </summary>
        /// <value>
        /// Ex: Locale. Example: "en-US".
        /// </value>
        public string Locale { get; set; }

        /// <summary>
        /// Gets or Sets Culture Model's InlineSeparator.
        /// </summary>
        /// <value>
        /// Ex: Locale. Example: ", ".
        /// </value>
        public string Separator { get; set; }

        /// <summary>
        /// Gets or Sets Culture Model's InlineOr.
        /// </summary>
        /// <value>
        /// Ex: Locale. Example: " or ".
        /// </value>
        public string InlineOr { get; set; }

        /// <summary>
        /// Gets or Sets Culture Model's InlineOrMore.
        /// </summary>
        /// <value>
        /// Ex: Locale. Example: ", or ".
        /// </value>
        public string InlineOrMore { get; set; }

        /// <summary>
        /// Gets or Sets Equivalent of "Yes" in Culture Model's Language.
        /// </summary>
        /// <value>
        /// Ex: Locale. Example: "Yes".
        /// </value>
        public string YesInLanguage { get; set; }

        /// <summary>
        /// Gets or Sets Equivalent of "No" in Culture Model's Language.
        /// </summary>
        /// <value>
        /// Ex: Locale. Example: "No".
        /// </value>
        public string NoInLanguage { get; set; }
    }
}
