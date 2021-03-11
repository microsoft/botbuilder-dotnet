// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// An <see cref="ResourceExplorer"/> implementation that uses a <see cref="FolderResourceProvider"/>.
    /// </summary>
    public class FolderResourceExplorer : ResourceExplorer
    {
        private const string ApplicationRootKey = "applicationRoot";

        private readonly FolderResourceProvider _folderResourceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderResourceExplorer"/> class.
        /// </summary>
        /// <param name="configuration">An <see cref="IConfiguration"/> instance.</param>
        public FolderResourceExplorer(IConfiguration configuration)
        {
            //var folder = configuration.GetSection(ApplicationRootKey).Value ?? throw new ArgumentException("Configuration missing application root key.");
            var folder = configuration.GetSection(ApplicationRootKey).Value ?? AppContext.BaseDirectory;
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
