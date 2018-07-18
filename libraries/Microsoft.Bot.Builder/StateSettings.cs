// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Describes state persistance options for use with state middleware.
    /// </summary>
    public class StateSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the middleware should set <see cref="IStoreItem.ETag"/> properties to "*".
        /// </summary>
        /// <value>
        /// A value indicating whether to set <see cref="IStoreItem.ETag"/> properties to "*".
        /// </value>
        public bool LastWriterWins { get; set; } = true;
    }
}
