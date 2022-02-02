using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Find QnAs that are associated with the given list of metadata. </summary>    
    public class MetadataFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataFilter"/> class. Initializes a new instance of metadata and logicalOperation.
        /// </summary>
        /// <param name="metadata">Dictionary of &lt;string&gt;.</param>
        /// <param name="logicalOperation">For joining metadata using &apos;OR&apos; operation.</param>
        public MetadataFilter(List<KeyValuePair<string, string>> metadata, string logicalOperation)
        {
            Metadata = metadata;
            LogicalOperation = logicalOperation;
        }

        /// <summary>Gets dictionary of &lt;string&gt;. </summary>
        /// <value>Dictionary of &lt;string&gt;.</value>
        public List<KeyValuePair<string, string>> Metadata { get; }

        /// <summary> Gets or sets to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation. (Optional).</summary>
        /// <value> For joining metadata using &apos;OR&apos; operation.</value>
        public string LogicalOperation { get; set; }
    }
}
