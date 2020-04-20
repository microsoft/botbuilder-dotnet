// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

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
            explorer.AddResourceProvider(new FolderResourceProvider(explorer, folder, includeSubFolders: includeSubFolders, monitorChanges: monitorChanges));
            return explorer;
        }

        /// <summary>
        ///  Add folder resources.
        /// </summary>
        /// <param name="explorer">The <see cref="ResourceExplorer"/> for this extension method.</param>
        /// <param name="folder">Collection of folders to include as resources.</param>
        /// <param name="ignoreFolders">Immediate sub-folders to ignore.</param>
        /// <param name="monitorChanges">Whether to track changes.</param>
        /// <returns>The resource explorer.</returns>
        public static ResourceExplorer AddFolders(this ResourceExplorer explorer, string folder, string[] ignoreFolders = null, bool monitorChanges = true)
        {
            if (ignoreFolders != null)
            {
                folder = PathUtils.NormalizePath(folder);

                explorer.AddFolder(folder, includeSubFolders: false, monitorChanges: monitorChanges);

                var hashIgnore = new HashSet<string>(ignoreFolders, StringComparer.CurrentCultureIgnoreCase);
                foreach (var subFolder in Directory.GetDirectories(folder).Where(f => !hashIgnore.Contains(Path.GetDirectoryName(f))))
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

        /// <summary>
        /// Add a .csproj as resource (adding the project, referenced projects and referenced packages).
        /// </summary>
        /// <param name="resourceExplorer">resource explorer.</param>
        /// <param name="projectFile">Project file.</param>
        /// <param name="ignoreFolders">Folders to ignore.</param>
        /// <param name="monitorChanges">Whether to track changes.</param>
        /// <returns>A new <see cref="ResourceExplorer"/>.</returns>
        public static ResourceExplorer LoadProject(this ResourceExplorer resourceExplorer, string projectFile, string[] ignoreFolders = null, bool monitorChanges = true)
        {
            projectFile = PathUtils.NormalizePath(projectFile);
            ignoreFolders = ignoreFolders?.Select(f => PathUtils.NormalizePath(f)).ToArray();

            if (!File.Exists(projectFile))
            {
                var foundProject = Directory.EnumerateFiles(projectFile, "*.*proj").FirstOrDefault();
                if (foundProject == null)
                {
                    resourceExplorer.AddFolder(Path.GetDirectoryName(projectFile), monitorChanges: monitorChanges);
                    return resourceExplorer;
                }
                else
                {
                    projectFile = foundProject;
                }
            }

            string projectFolder = Path.GetDirectoryName(projectFile);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(projectFile);

            // add folder for the project
            if (ignoreFolders != null)
            {
                resourceExplorer.AddFolders(projectFolder, ignoreFolders, monitorChanges: monitorChanges);
            }
            else
            {
                resourceExplorer.AddResourceProvider(new FolderResourceProvider(resourceExplorer, projectFolder, includeSubFolders: true, monitorChanges: monitorChanges));
            }

            // add project references
            foreach (XmlNode node in xmlDoc.SelectNodes("//ProjectReference"))
            {
                var path = Path.Combine(projectFolder, PathUtils.NormalizePath(node.Attributes["Include"].Value));
                path = Path.GetFullPath(path);
                path = Path.GetDirectoryName(path);
                if (Directory.Exists(path))
                {
                    resourceExplorer.AddResourceProvider(new FolderResourceProvider(resourceExplorer, path, includeSubFolders: true, monitorChanges: monitorChanges));
                }
            }

            var packages = Path.GetFullPath("packages");
            var relativePackagePath = Path.Combine(@"..", "packages");
            while (!Directory.Exists(packages) && Path.GetDirectoryName(packages) != Path.GetPathRoot(packages))
            {
                packages = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(packages), PathUtils.NormalizePath(relativePackagePath)));
                if (packages == null)
                {
                    throw new ArgumentNullException("Can't find packages folder");
                }
            }

            var pathResolver = new PackagePathResolver(packages);

            // add NuGet package references
            foreach (XmlNode node in xmlDoc.SelectNodes("//PackageReference"))
            {
                string packageName = node.Attributes["Include"]?.Value;
                string version = node.Attributes["Version"]?.Value;
                NuGetVersion nugetVersion;
                if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(version) && NuGetVersion.TryParse(version, out nugetVersion))
                {
                    var package = new PackageIdentity(packageName, nugetVersion);
                    var folder = Path.Combine(packages, PathUtils.NormalizePath(pathResolver.GetPackageDirectoryName(package)));
                    if (Directory.Exists(folder))
                    {
                        resourceExplorer.AddResourceProvider(new FolderResourceProvider(resourceExplorer, folder, includeSubFolders: true, monitorChanges: monitorChanges));
                    }
                }
            }

            return resourceExplorer;
        }
    }
}
