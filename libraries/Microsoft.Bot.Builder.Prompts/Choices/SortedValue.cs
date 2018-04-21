// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Prompts.Choices
{
    /// <summary>
    /// A value that can be sorted and still refer to its original position with a source array.
    /// </summary>
    public class SortedValue
    {
        ///<summary>
        /// The value that will be sorted.
        ///</summary>
        public string Value { get; set; }

        ///<summary>
        /// The values original position within its unsorted array.
        ///</summary>
        public int Index { get; set; }
    }
}
