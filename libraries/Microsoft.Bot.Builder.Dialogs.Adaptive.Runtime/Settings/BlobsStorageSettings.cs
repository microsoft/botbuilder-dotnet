// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Settings
{
    /// <summary>
    /// Settings for blob storage.
    /// </summary>
    internal class BlobsStorageSettings
    {
        /// <summary>
        /// Gets or sets the blob connection string.
        /// </summary>
        /// <value>
        /// The blob connection string.
        /// </value>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the blob container name.
        /// </summary>
        /// <value>
        /// The blob container name.
        /// </value>
        public string ContainerName { get; set; }
    }
}
