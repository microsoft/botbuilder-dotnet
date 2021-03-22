// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// An <see cref="ResourceExplorer"/> implementation that uses a <see cref="FolderResourceProvider"/>.
    /// </summary>
    public class FolderResourceExplorer : ResourceExplorer
    {
        private readonly FolderResourceProvider _folderResourceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderResourceExplorer"/> class.
        /// </summary>
        /// <param name="folder">The folder from which to load resources.</param>
        public FolderResourceExplorer(string folder)
        {
            _folderResourceProvider = new FolderResourceProvider(this, folder, includeSubFolders: true, monitorChanges: true);
            AddResourceProvider(_folderResourceProvider);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _folderResourceProvider.Dispose();
            base.Dispose(disposing);
        }
    }
}
