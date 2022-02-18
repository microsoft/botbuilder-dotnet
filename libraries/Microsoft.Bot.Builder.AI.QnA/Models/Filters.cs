// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Filters over knowledge base. </summary>
    public class Filters
    {
        /// <summary> Gets or sets QnAs that are associated with the given list of metadata. </summary>
        /// <value> QnAs that are associated with the given lit of metadata.</value>
        public MetadataFilter MetadataFilter { get; set; }

        /// <summary> Gets QnAs that are associated with the given list of sources in knowledge base. </summary>
        /// <value> QnAs that are associated with the given list of sources in knowledge base.</value>
        public List<string> SourceFilter { get; } = new List<string>();

        /// <summary> Gets or sets to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation (Optional).</summary>
        /// <value> For joining metadata using &apos;OR&apos; operation.</value>
        public string LogicalOperation { get; set; }
    }
}
