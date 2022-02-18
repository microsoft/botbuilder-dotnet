// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.AI.QnA.Models
{
    /// <summary> Find QnAs that are associated with the given list of metadata. </summary>    
    public class MetadataFilter
    {
        /// <summary>Gets dictionary of &lt;string&gt;. </summary>
        /// <value>Dictionary of &lt;string&gt;.</value>
        public List<KeyValuePair<string, string>> Metadata { get; } = new List<KeyValuePair<string, string>>();

        /// <summary> Gets or sets to &apos;OR&apos; for joining metadata using &apos;OR&apos; operation. (Optional).</summary>
        /// <value> For joining metadata using &apos;OR&apos; operation.</value>
        public string LogicalOperation { get; set; }
    }
}
