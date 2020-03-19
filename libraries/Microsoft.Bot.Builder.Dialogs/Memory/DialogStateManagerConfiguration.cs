using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Bot.Builder.Dialogs.Memory.Scopes;

namespace Microsoft.Bot.Builder.Dialogs.Memory
{
    public class DialogStateManagerConfiguration
    {
        /// <summary>
        /// Gets or sets PathResolvers.
        /// </summary>
        /// <value>
        /// PathResolvers (aka shortcuts) to load into the dialog state manager context.
        /// </value>
        public List<IPathResolver> PathResolvers { get; set; } = new List<IPathResolver>();

        /// <summary>
        /// Gets or sets MemoryScopes.
        /// </summary>
        /// <value>
        /// MemoryScopes to load into the dialog state manager context.
        /// </value>
        public List<MemoryScope> MemoryScopes { get; set; } = new List<MemoryScope>();
    }
}
