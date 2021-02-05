// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Builder.Runtime.Settings
{
    /// <summary>
    /// Settings for composer resources such as adapters and storage.
    /// </summary>
    internal class ResourcesSettings
    {
        /// <summary>
        /// Gets or sets the name of the storage to use.
        /// </summary>
        /// <value>
        /// The name of the storage to use.
        /// </value>
        public string Storage { get; set; }

        /// <summary>
        /// Gets or sets the list of adapters to expose in the runtime.
        /// </summary>
        /// <value>
        /// The list of adapters to expose in the runtime.
        /// </value>
        public IList<AdapterSettings> Adapters { get; set; } = new List<AdapterSettings>();
    }
}
