using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Find QnAs that are associated with the given list of metadata. </summary>    
    public class MetadataFilter
    {
        /// <summary> Dictionary of &lt;string&gt;. </summary>
        public List<KeyValuePair<string, string>> Metadata { get; set; }

        /// <summary> (Optional) Set to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation. </summary>
        public string LogicalOperation { get; set; }
    }

}
