using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Debugging
{
    /// <summary>
    /// Resource-aware specialization of <see cref="SourceContext"/>.
    /// </summary>
    internal class ResourceSourceContext : SourceContext
    {
        /// <summary>
        /// Gets a mapping between <see cref="JToken" /> and their respective resource ids. 
        /// </summary>
        /// <value>
        /// A mapping between <see cref="JToken" /> and their respective resource ids.
        /// </value>
        internal Dictionary<JToken, string> ResourceIdMap { get; } = new Dictionary<JToken, string>(new JTokenEqualityComparer());
    }
}
