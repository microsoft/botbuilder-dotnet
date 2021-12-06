using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Find QnAs that are associated with the given list of metadata. </summary>    
    public class MetadataFilter
    {
        /// <summary> Gets or sets dictionary of &lt;string&gt;. </summary>
        /// <value>
        ///  A value set to array of key value pairs of strings. 
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only
        public List<KeyValuePair<string, string>> Metadata { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary> Gets or sets LogicalOperation to &apos;OR&apos; or &apos;AND&apos; for joining metadata. </summary>
        /// <value>
        ///  A value set to &apos;OR&apos; or &apos;AND&apos; for joining metadata using &apos;OR&apos; operation. 
        /// </value>
        public string LogicalOperation { get; set; }
    }
}
