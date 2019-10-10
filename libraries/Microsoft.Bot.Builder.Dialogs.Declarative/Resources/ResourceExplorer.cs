// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public delegate void ResourceChangedEventHandler(IResource[] resources);

    /// <summary>
    /// Class which gives standard access to file based resources.
    /// </summary>
    public class ResourceExplorer : IDisposable
    {
        private List<IResourceProvider> resourceProviders = new List<IResourceProvider>();

        private CancellationTokenSource cancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<IResource> changedResources = new ConcurrentBag<IResource>();

        public ResourceExplorer()
        {
        }

        /// <summary>
        /// Event which fires when a resource is changed.
        /// </summary>
        public event ResourceChangedEventHandler Changed;

        /// <summary>
        /// Gets the resource providers.
        /// </summary>
        public IEnumerable<IResourceProvider> ResourceProviders
        {
            get { return this.resourceProviders; }
        }

        /// <summary>
        /// Add a .csproj as resource (adding the project, referenced projects and referenced packages).
        /// </summary>
        /// <param name="projectFile">Project file.</param>
        /// <param name="ignoreFolders">Folders to ignore.</param>
        /// <param name="monitorChanges">Whether to track changes.</param>
        /// <returns>A new <see cref="ResourceExplorer"/>.</returns>
        public static ResourceExplorer LoadProject(string projectFile, string[] ignoreFolders = null, bool monitorChanges = true)
        {
            var explorer = new ResourceExplorer();
            projectFile = PathUtils.NormalizePath(projectFile);
            ignoreFolders = ignoreFolders?.Select(f => PathUtils.NormalizePath(f)).ToArray();

            if (!File.Exists(projectFile))
            {
                projectFile = Directory.EnumerateFiles(projectFile, "*.*proj").FirstOrDefault();
                if (projectFile == null)
                {
                    explorer.AddFolder(Path.GetDirectoryName(projectFile));
                    return explorer;
                }
            }

            string projectFolder = Path.GetDirectoryName(projectFile);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(projectFile);

            // add folder for the project
            if (ignoreFolders != null)
            {
                explorer.AddFolders(projectFolder, ignoreFolders, monitorChanges: monitorChanges);
            }
            else
            {
                explorer.AddResourceProvider(new FolderResourceProvider(projectFolder, includeSubFolders: true, monitorChanges: monitorChanges));
            }

            // add project references
            foreach (XmlNode node in xmlDoc.SelectNodes("//ProjectReference"))
            {
                var path = Path.Combine(projectFolder, PathUtils.NormalizePath(node.Attributes["Include"].Value));
                path = Path.GetFullPath(path);
                path = Path.GetDirectoryName(path);
                if (Directory.Exists(path))
                {
                    explorer.AddResourceProvider(new FolderResourceProvider(path, includeSubFolders: true, monitorChanges: monitorChanges));
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

            // add nuget package references
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
                        explorer.AddResourceProvider(new FolderResourceProvider(folder, includeSubFolders: true, monitorChanges: monitorChanges));
                    }
                }
            }

            return explorer;
        }

        public ResourceExplorer AddResourceProvider(IResourceProvider resourceProvider)
        {
            resourceProvider.Changed += ResourceProvider_Changed;

            if (this.resourceProviders.Any(r => r.Id == resourceProvider.Id))
            {
                throw new ArgumentException($"{resourceProvider.Id} has already been added as a resource");
            }

            this.resourceProviders.Add(resourceProvider);
            return this;
        }

        /// <summary>
        /// Get resources of a given type.
        /// </summary>
        /// <param name="fileExtension">File extension filter.</param>
        /// <returns>The resources.</returns>
        public IEnumerable<IResource> GetResources(string fileExtension)
        {
            foreach (var resourceProvider in this.resourceProviders)
            {
                foreach (var resource in resourceProvider.GetResources(fileExtension))
                {
                    yield return resource;
                }
            }
        }

        /// <summary>
        /// Get resource by id
        /// </summary>
        /// <param name="id">The resource id .</param>
        /// <returns>The resource, or throws if not found.</returns>
        public IResource GetResource(string id)
        {
            if (TryGetResource(id, out var resource))
            {
                return resource;
            }

            throw new ArgumentException($"Could not find resource '{id}'", paramName: id);
        }

        /// <summary>
        /// Try to get the resource by id
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="resource">resource that was found or null</param>
        /// <returns>true if found</returns>
        public bool TryGetResource(string id, out IResource resource)
        {
            foreach (var resourceProvider in this.resourceProviders)
            {
                if (resourceProvider.TryGetResource(id, out resource))
                {
                    return true;
                }
            }

            resource = null;
            return false;
        }

        public void Dispose()
        {
            foreach (var resource in this.resourceProviders)
            {
                if (resource is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private void ResourceProvider_Changed(IResource[] resources)
        {
            if (this.Changed != null)
            {
                foreach (var resource in resources)
                {
                    changedResources.Add(resource);
                }

                lock (cancelReloadToken)
                {
                    cancelReloadToken.Cancel();
                    cancelReloadToken = new CancellationTokenSource();
                    Task.Delay(1000, cancelReloadToken.Token)
                        .ContinueWith(t =>
                        {
                            if (t.IsCanceled)
                            {
                                return;
                            }

                            var changed = changedResources.ToArray();
                            changedResources = new ConcurrentBag<IResource>();
                            this.Changed(changed);
                        }).ContinueWith(t => t.Status);
                }
            }
        }
    }
}
