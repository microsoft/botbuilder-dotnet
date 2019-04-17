// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    public delegate void ResourceChangedEventHandler(string[] paths);

    /// <summary>
    /// Class which gives standard access to file based resources
    /// </summary>
    public class ResourceExplorer : IDisposable
    {
        private List<IResourceProvider> resourceProviders = new List<IResourceProvider>();

        private CancellationTokenSource CancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<string> changedPaths = new ConcurrentBag<string>();

        public ResourceExplorer()
        {
        }

        public IEnumerable<IResourceProvider> ResourceProviders { get { return this.resourceProviders; } }

        public event ResourceChangedEventHandler Changed;

        public ResourceExplorer AddResourceProvider(IResourceProvider resourceProvider)
        {
            resourceProvider.Changed += ResourceProvider_Changed;

            this.resourceProviders.Add(resourceProvider);
            return this;
        }

        private void ResourceProvider_Changed(string[] ids)
        {
            if (this.Changed != null)
            {
                foreach (var id in ids)
                {
                    changedPaths.Add(id);
                }

                lock (CancelReloadToken)
                {
                    CancelReloadToken.Cancel();
                    CancelReloadToken = new CancellationTokenSource();
                    Task.Delay(1000, CancelReloadToken.Token)
                        .ContinueWith(t =>
                        {
                            if (t.IsCanceled)
                                return;
                            var changed = changedPaths.ToArray();
                            changedPaths = new ConcurrentBag<string>();
                            this.Changed(changed);
                        }).ContinueWith(t => t.Status);
                }
            }
        }

        /// <summary>
        /// Add a .csproj as resource (adding the project, referenced projects and referenced packages)
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="projectFile"></param>
        /// <returns></returns>
        public static ResourceExplorer LoadProject(string projectFile, string[] ignoreFolders = null)
        {
            var explorer = new ResourceExplorer();
            if (!File.Exists(projectFile))
            {
                projectFile = Directory.EnumerateFiles(projectFile, "*.*proj").FirstOrDefault();
                if (projectFile == null)
                {
                    throw new ArgumentNullException(nameof(projectFile));
                }
            }
            string projectFolder = Path.GetDirectoryName(projectFile);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(projectFile);

            // add folder for the project
            if (ignoreFolders != null)
            {
                explorer.AddFolders(projectFolder, ignoreFolders, monitorChanges: true);
            }
            else
            {
                explorer.AddResourceProvider(new FolderResourceProvider(projectFolder, includeSubFolders: true, monitorChanges: true));
            }

            // add project references
            foreach (XmlNode node in xmlDoc.SelectNodes("//ProjectReference"))
            {
                var path = Path.Combine(projectFolder, PlatformPath(node.Attributes["Include"].Value));
                path = Path.GetFullPath(path);
                path = Path.GetDirectoryName(path);
                explorer.AddResourceProvider(new FolderResourceProvider(path, includeSubFolders: true, monitorChanges: true));
            }

            var packages = Path.GetFullPath("packages");
            var relativePackagePath = Path.Combine(@"..", "packages");
            while (!Directory.Exists(packages) && Path.GetDirectoryName(packages) != Path.GetPathRoot(packages))
            {
                packages = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(packages), PlatformPath(relativePackagePath)));
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
                if (!String.IsNullOrEmpty(packageName) && !String.IsNullOrEmpty(version))
                {
                    var package = new PackageIdentity(packageName, new NuGetVersion(version));
                    var folder = Path.Combine(packages, PlatformPath(pathResolver.GetPackageDirectoryName(package)));
                    if (Directory.Exists(folder))
                    {
                        explorer.AddResourceProvider(new FolderResourceProvider(folder, includeSubFolders: true, monitorChanges: false));
                    }
                }
            }

            return explorer;
        }

        private static string PlatformPath(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }
            else
            {
                return path.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }
        }

        /// <summary>
        /// get resources of a given type
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
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
        /// Get resource by filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public IResource GetResource(string filename)
        {
            return GetResources(Path.GetExtension(filename)).Where(resource => resource.Id == filename).SingleOrDefault();
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
    }
}
