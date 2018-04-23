// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class FoundValue
    {
        ///<summary>
        /// The value that was matched.
        ///</summary>
        public string Value { get; set; }

        ///<summary>
        /// The index of the value that was matched.
        ///</summary>
        public int Index { get; set; }

        ///<summary>
        /// The accuracy with which the value matched the specified portion of the utterance. A
        /// value of 1.0 would indicate a perfect match.
        ///</summary>
        public float Score { get; set; }
    }
}
