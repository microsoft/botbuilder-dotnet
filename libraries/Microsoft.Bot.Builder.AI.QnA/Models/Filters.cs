using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> filters over knowledge base. </summary>
    public class Filters
    {
        /// <summary> Initializes a new instance of StrictFilters. </summary>
        public Filters()
        {
            SourceFilter = new List<string>();
        }

        /// <summary> Find QnAs that are associated with the given list of metadata. </summary>
        public MetadataFilter MetadataFilter { get; set; }
        /// <summary> Find QnAs that are associated with the given list of sources in knowledge base. </summary>
        public List<string> SourceFilter { get; set; }
        /// <summary> (Optional) Set to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation. </summary>
        public string LogicalOperation { get; set; }
    }
}
