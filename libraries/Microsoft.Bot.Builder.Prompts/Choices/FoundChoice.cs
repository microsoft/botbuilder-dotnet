// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    public class FoundChoice
    {
        ///<summary>
        /// The value of the choice that was matched.
        ///</summary>
        public string Value { get; set; }

        ///<summary>
        /// The choices index within the list of choices that was searched over.
        ///</summary>
        public int Index { get; set; }

        ///<summary>
        /// The accuracy with which the synonym matched the specified portion of the utterance. A
        /// value of 1.0 would indicate a perfect match.
        ///</summary>
        public float Score { get; set; }

        ///<summary>
        /// (Optional) The synonym that was matched.
        ///</summary>
        public string Synonym { get; set; }
    }
}
