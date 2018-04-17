// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class FindValuesOptions
    {
        ///<summary>
        /// (Optional) if true, then only some of the tokens in a value need to exist to be considered
        /// a match. The default value is "false".
        ///</summary>
        public bool AllowPartialMatches { get; set; }

        ///<summary>
        /// (Optional) locale/culture code of the utterance. The default is `en-US`.
        ///</summary>
        public string Locale { get; set; }

        ///<summary>
        /// (Optional) maximum tokens allowed between two matched tokens in the utterance. So with
        /// a max distance of 2 the value "second last" would match the utterance "second from the last"
        /// but it wouldn't match "Wait a second. That's not the last one is it?".
        /// The default value is "2".  
        ///</summary>
        public int? MaxTokenDistance { get; set; }

        ///<summary>
        /// (Optional) tokenizer to use when parsing the utterance and values being recognized.
        ///</summary>
        public TokenizerFunction Tokenizer { get; set; }
    }
}
