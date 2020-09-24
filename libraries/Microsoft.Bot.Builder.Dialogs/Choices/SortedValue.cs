// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Choices
{
    /// <summary>
    /// A value that can be sorted and still refer to its original position with a source array.
    /// </summary>
    public class SortedValue
    {
        /// <summary>
        /// Gets or sets the value that will be sorted.
        /// </summary>
        /// <value>
        /// The value that will be sorted.
        /// </value>
        [JsonProperty("value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the values original position within its unsorted array.
        /// </summary>
        /// <value>
        /// The values original position within its unsorted array.
        /// </value>
        [JsonProperty("index")]
        public int Index { get; set; }
    }
}
