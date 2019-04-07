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

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Class which gives standard access to file based resources
    /// </summary>
    public class ResourceExplorer : IDisposable
    {
        private List<FolderResource> folderResources = new List<FolderResource>();

        public ResourceExplorer()
        {
        }


        public IEnumerable<DirectoryInfo> Folders
        {
            get
            {
                foreach (var folderResource in folderResources)
                {
                    yield return folderResource.Directory;
                }
            }
        }

        public event ResourceChangedEventHandler Changed;

        private CancellationTokenSource CancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<string> changedPaths = new ConcurrentBag<string>();

        public ResourceExplorer AddFolder(string folder, bool monitorFiles = true)
        {
            var folderResource = new FolderResource(folder, monitorFiles);

            if (folderResource.Watcher != null)
            {
                folderResource.Watcher.Created += Watcher_Changed;
                folderResource.Watcher.Changed += Watcher_Changed;
                folderResource.Watcher.Deleted += Watcher_Changed;
            }

            this.folderResources.Add(folderResource);
            return this;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.Changed != null)
            {
                changedPaths.Add(e.FullPath);
                lock (CancelReloadToken)
                {
                    CancelReloadToken.Cancel();
                    CancelReloadToken = new CancellationTokenSource();
                    Task.Delay(1000, CancelReloadToken.Token)
                        .ContinueWith(t =>
                        {
                            if (t.IsCanceled)
                                return;

                            this.Changed(changedPaths.ToArray());
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
        public static ResourceExplorer LoadProject(string projectFile)
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
            explorer.AddFolder(projectFolder);

            // add project references
            foreach (XmlNode node in xmlDoc.SelectNodes("//ProjectReference"))
            {
                var path = Path.Combine(projectFolder, node.Attributes["Include"].Value);
                path = Path.GetFullPath(path);
                path = Path.GetDirectoryName(path);
                explorer.AddFolder(path);
            }

            var packages = Path.GetFullPath("packages");
            var relativePackagePath = Path.Combine(@"..", "packages");
            while (!Directory.Exists(packages) && Path.GetDirectoryName(packages) != Path.GetPathRoot(packages))
            {
                packages = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(packages), relativePackagePath));
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
                    var folder = Path.Combine(packages, pathResolver.GetPackageDirectoryName(package));
                    if (Directory.Exists(folder))
                    {
                        explorer.AddFolder(folder, monitorFiles: false);
                    }
                }
            }

            return explorer;
        }

        /// <summary>
        /// get resources of a given type
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public IEnumerable<FileInfo> GetResources(string fileExtension)
        {
            foreach (var folder in this.folderResources)
            {
                foreach (var fileInfo in folder.GetResources(fileExtension))
                {
                    yield return fileInfo;
                }
            }
        }

        /// <summary>
        /// Get resource by filename
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public FileInfo GetResource(string filename)
        {
            return GetResources(Path.GetExtension(filename)).Where(fi => fi.Name == filename).SingleOrDefault();
        }

        public void Dispose()
        {
            foreach (var folderResource in this.folderResources)
            {
                folderResource.Dispose();
            }
        }

        /// <summary>
        /// Folder/FileResources
        /// </summary>
        internal class FolderResource : IDisposable
        {
            internal FolderResource(string folder, bool monitorChanges = true)
            {
                this.Directory = new DirectoryInfo(folder);
                if (monitorChanges)
                {
                    this.Watcher = new FileSystemWatcher(folder);
                    this.Watcher.IncludeSubdirectories = true;
                    this.Watcher.EnableRaisingEvents = true;
                }
            }

            /// <summary>
            /// folder to enumerate
            /// </summary>
            public DirectoryInfo Directory { get; set; }

            public FileSystemWatcher Watcher { get; private set; }

            public void Dispose()
            {
                lock (Directory)
                {
                    if (Watcher != null)
                    {
                        Watcher.EnableRaisingEvents = false;
                        Watcher.Dispose();
                        Watcher = null;
                    }
                }
            }

            /// <summary>
            /// id -> Resource object)
            /// </summary>
            public IEnumerable<FileInfo> GetResources(string extension)
            {
                foreach (var fileInfo in this.Directory.EnumerateFiles($"*.{extension.TrimStart('.')}", SearchOption.AllDirectories))
                {
                    yield return fileInfo;
                }
                yield break;
            }
        }


    }
}
