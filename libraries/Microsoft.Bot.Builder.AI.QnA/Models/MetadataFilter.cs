// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Find QnAs that are associated with the given list of metadata. </summary>    
    public class MetadataFilter
    {
        /// <summary>Gets list of dictionary of <see cref="string"/> which finds QnAs that are associated with the given list of metadata..</summary>
        /// <value>List of dictionary of <see cref="string"/>.</value>
        public List<KeyValuePair<string, string>> Metadata { get; } = new List<KeyValuePair<string, string>>();

        /// <summary> Gets or sets logical operation for metadata filters.</summary>
        /// <value>OR/AND, defaults to OR.</value>
        public string LogicalOperation { get; set; }
    }
}
