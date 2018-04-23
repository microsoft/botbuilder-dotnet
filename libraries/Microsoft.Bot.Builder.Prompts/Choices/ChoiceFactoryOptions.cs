// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class ChoiceFactoryOptions
    {
        ///<summary>
        /// (Optional) character used to separate individual choices when there are more than 2 choices.
        /// The default value is `", "`.
        ///</summary>
        public string InlineSeparator { get; set; }

        ///<summary>
        /// (Optional) separator inserted between the choices when their are only 2 choices. The default
        /// value is `" or "`.
        ///</summary>
        public string InlineOr { get; set; }

        ///<summary>
        /// (Optional) separator inserted between the last 2 choices when their are more than 2 choices.
        /// The default value is `", or "`.
        ///</summary>
        public string InlineOrMore { get; set; }

        ///<summary>
        /// (Optional) if `true`, inline and list style choices will be prefixed with the index of the
        /// choice as in "1. choice". If `false`, the list style will use a bulleted list instead.The default value is `true`.
        ///</summary>
        public bool? IncludeNumbers { get; set; }
    }
}
