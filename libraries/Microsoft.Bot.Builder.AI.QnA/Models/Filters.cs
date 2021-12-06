using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> filters over knowledge base. </summary>
    public class Filters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Filters"/> class. </summary>
        public Filters()
        {
            SourceFilter = new List<string>();
        }

        /// <summary> Gets or sets MetadataFilter. </summary>
        /// <value>A value used to filter QnAs.</value>
        public MetadataFilter MetadataFilter { get; set; }

        /// <summary> Gets or sets list of sources in knowledge base to be used for filtering. </summary>
        /// <value>
        ///  List of sources in knowledge base to be used for filtering. 
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<string> SourceFilter { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary> Gets or sets (Optional) Set to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation. </summary>
        /// <value>
        ///  (Optional) Set to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation. 
        /// </value>
        public string LogicalOperation { get; set; }
    }
}
