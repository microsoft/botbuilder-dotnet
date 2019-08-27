// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Folder/FileResources.
    /// </summary>
    public static class FolderResourceProviderExtensions
    {
        /// <summary>
        /// Add a folder resource.
        /// </summary>
        /// <param name="explorer">The <see cref="ResourceExplorer"/> for this extension method.</param>
        /// <param name="folder">Folder to include as a resource.</param>
        /// <param name="includeSubFolders">Whether to include subfolders.</param>
        /// <param name="monitorChanges">Whether to track changes.</param>
        /// <returns>The resource explorer.</returns>
        public static ResourceExplorer AddFolder(this ResourceExplorer explorer, string folder, bool includeSubFolders = true, bool monitorChanges = true)
        {
            explorer.AddResourceProvider(new FolderResourceProvider(folder, includeSubFolders: includeSubFolders, monitorChanges: monitorChanges));
            return explorer;
        }

        /// <summary>
        ///  Add folder resources.
        /// </summary>
        /// <param name="explorer">The <see cref="ResourceExplorer"/> for this extension method.</param>
        /// <param name="folder">Collection of folders to include as resources.</param>
        /// <param name="ignoreFolders">Folders to ignore.</param>
        /// <param name="monitorChanges">Whether to track changes.</param>
        /// <returns>The resource explorer.</returns>
        public static ResourceExplorer AddFolders(this ResourceExplorer explorer, string folder, string[] ignoreFolders = null, bool monitorChanges = true)
        {
            if (ignoreFolders != null)
            {
                folder = PathUtils.NormalizePath(folder);

                explorer.AddFolder(folder, includeSubFolders: false, monitorChanges: monitorChanges);

                var hashIgnore = new HashSet<string>(ignoreFolders.Select(p => Path.Combine(folder, p)), StringComparer.CurrentCultureIgnoreCase);
                foreach (var subFolder in Directory.GetDirectories(folder).Where(f => !hashIgnore.Contains(f)))
                {
                    // add subfolders not in ignore list
                    explorer.AddFolder(subFolder, includeSubFolders: true, monitorChanges: monitorChanges);
                }
            }
            else
            {
                explorer.AddFolder(folder, includeSubFolders: true, monitorChanges: monitorChanges);
            }

            return explorer;
        }
    }
}
