// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Filters over knowledge base. </summary>
    public class Filters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Filters"/> class. Initializes a new instance of sourceFilter.
        /// </summary>
        public Filters()
        {
            SourceFilter = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Filters"/> class. Initializes a new instance of metadataFilter, sourceFilter and logicalOperation.
        /// </summary>
        /// <param name="metadataFilter">QnAs that are associated with the given lit of metadata.</param>
        /// <param name="sourceFilter">QnAs that are associated with the given list of sources in knowledge base.</param>
        /// <param name="logicalOperation">For joining metadata using &apos;OR&apos; operation.</param>
        public Filters(MetadataFilter metadataFilter, List<string> sourceFilter, string logicalOperation)
        {
            MetadataFilter = metadataFilter;
            SourceFilter = sourceFilter;
            LogicalOperation = logicalOperation;
        }

        /// <summary> Gets or sets QnAs that are associated with the given list of metadata. </summary>
        /// <value> QnAs that are associated with the given lit of metadata.</value>
        public MetadataFilter MetadataFilter { get; set; }

        /// <summary> Gets QnAs that are associated with the given list of sources in knowledge base. </summary>
        /// <value> QnAs that are associated with the given list of sources in knowledge base.</value>
        public List<string> SourceFilter { get; }

        /// <summary> Gets or sets to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation (Optional).</summary>
        /// <value> For joining metadata using &apos;OR&apos; operation.</value>
        public string LogicalOperation { get; set; }
    }
}
