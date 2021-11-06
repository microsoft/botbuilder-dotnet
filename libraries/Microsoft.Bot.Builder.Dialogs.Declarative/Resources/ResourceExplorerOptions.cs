// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Configuration options for <see cref="ResourceExplorer"/>.
    /// </summary>
    public class ResourceExplorerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether whether cycles are allowed in resources managed by the <see cref="ResourceExplorer"/>.
        /// </summary>
        /// <value>
        /// Whether cycles are allowed in resources managed by the <see cref="ResourceExplorer"/>.
        /// </value>
        public bool AllowCycles { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of resource providers to initialize the current the <see cref="ResourceExplorer"/>.
        /// </summary>
        /// <value>
        /// The list of resource providers to initialize the current the <see cref="ResourceExplorer"/>.
        /// </value>
        public IEnumerable<ResourceProvider> Providers { get; set; }

        /// <summary>
        /// Gets or sets the list of declarative types to use. Falls back to <see cref="ComponentRegistration.Components" /> if set to null.
        /// </summary>
        /// <value>
        /// The list of declarative types to use. Falls back to <see cref="ComponentRegistration.Components" /> if set to null.
        /// </value>
        public IEnumerable<IComponentDeclarativeTypes> DeclarativeTypes { get; set; }
    }   
}
