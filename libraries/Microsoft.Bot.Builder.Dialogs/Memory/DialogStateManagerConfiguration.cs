// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
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
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<IPathResolver> PathResolvers { get; set; } = new List<IPathResolver>();
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Gets or sets MemoryScopes.
        /// </summary>
        /// <value>
        /// MemoryScopes to load into the dialog state manager context.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public List<MemoryScope> MemoryScopes { get; set; } = new List<MemoryScope>();
#pragma warning restore CA2227 // Collection properties should be read only
    }
}
