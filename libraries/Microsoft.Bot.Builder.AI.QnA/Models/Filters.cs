// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Filter QnAs based on given metadata list and knowledge base sources. </summary>
    public class Filters
    {
        /// <summary> Gets or sets QnAs that are associated with the given list of metadata. </summary>
        /// <value> Object to provide the key value pair for each <see cref="Metadata"/>.</value>
        public MetadataFilter MetadataFilter { get; set; }

        /// <summary> Gets QnAs that are associated with the given list of sources in knowledge base. </summary>
        /// <value> List of sources in knowledge base.</value>
        public List<string> SourceFilter { get; } = new List<string>();

        /// <summary> Gets or sets Logical operation used to join metadata filter with source filter.</summary>
        /// <value> OR/AND, default is OR.</value>
        public string LogicalOperation { get; set; }
    }
}
