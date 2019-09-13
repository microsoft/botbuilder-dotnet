// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{

    /// <summary>
    /// Folder/FileResources
    /// </summary>
    public class FolderResourceProvider : IResourceProvider, IDisposable
    {
        private CancellationTokenSource CancelReloadToken = new CancellationTokenSource();
        private ConcurrentBag<string> changedPaths = new ConcurrentBag<string>();
        private FileSystemWatcher Watcher;
        private Dictionary<string, FileResource> resources = new Dictionary<string, FileResource>();
        private HashSet<string> extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public FolderResourceProvider(string folder, bool includeSubFolders = true, bool monitorChanges = true)
        {
            foreach (var extension in new string[] { ".lg", ".lu", ".dialog", ".schema", ".md", ".form" })
            {
                this.extensions.Add(extension);
            }

            this.IncludeSubFolders = includeSubFolders;
            folder = PathUtils.NormalizePath(folder);
            this.Directory = new DirectoryInfo(folder);
            SearchOption option = (this.IncludeSubFolders) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var fileInfo in this.Directory.EnumerateFiles($"*.*", option).Where(fi => this.extensions.Contains(fi.Extension)))
            {
                var fileResource = new FileResource(fileInfo.FullName);
                this.resources[fileResource.Id] = fileResource;
            }

            if (monitorChanges)
            {
                this.Watcher = new FileSystemWatcher(folder);
                this.Watcher.IncludeSubdirectories = this.IncludeSubFolders;
                this.Watcher.EnableRaisingEvents = true;
                this.Watcher.Created += Watcher_Changed;
                this.Watcher.Changed += Watcher_Changed;
                this.Watcher.Deleted += Watcher_Deleted;
                this.Watcher.Renamed += Watcher_Renamed;
            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            var ext = Path.GetExtension(e.FullPath);
            if (this.extensions.Contains(ext))
            {
                lock (this.resources)
                {
                    this.resources.Remove(Path.GetFileName(e.FullPath));
                    if (this.Changed != null)
                    {
                        this.Changed(new IResource[] { new FileResource(e.FullPath) });
                    }
                }
            }
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            var ext = Path.GetExtension(e.FullPath);
            if (this.extensions.Contains(ext))
            {
                lock (this.resources)
                {
                    var fileResource = new FileResource(e.FullPath);
                    this.resources[fileResource.Id] = fileResource;
                    if (this.Changed != null)
                    {
                        this.Changed(new IResource[] { fileResource });
                    }
                }
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var ext = Path.GetExtension(e.FullPath);
            if (this.extensions.Contains(ext))
            {
                var fileResource = new FileResource(e.FullPath);

                lock (this.resources)
                {
                    this.resources[fileResource.Id] = fileResource;
                }

                if (this.Changed != null)
                {
                    this.Changed(new IResource[] { fileResource });
                }
            }
        }

        /// <summary>
        /// folder to enumerate
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        public bool IncludeSubFolders { get; set; }

        public string Id { get { return this.Directory.FullName; } }

        public event ResourceChangedEventHandler Changed;

        public void Dispose()
        {
            lock (Directory)
            {
                if (this.Watcher != null)
                {
                    this.Watcher.EnableRaisingEvents = false;
                    this.Watcher.Dispose();
                    this.Watcher = null;
                }
            }
        }


        /// <summary>
        /// GetResource by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IResource GetResource(string id)
        {
            lock (this.resources)
            {
                if (this.resources.TryGetValue(id, out FileResource fileResource))
                {
                    return fileResource;
                }

                return null;
            }
        }

        /// <summary>
        /// Get Resources by extension
        /// </summary>
        public IEnumerable<IResource> GetResources(string extension)
        {
            extension = $".{extension.TrimStart('.').ToLower()}";

            lock (this.resources)
            {
                return this.resources.Where(pair => Path.GetExtension(pair.Key).ToLower() == extension).Select(pair => pair.Value).ToList();
            }
        }

        public override string ToString()
        {
            return this.Id;
        }
    }

    public static class FolderResourceProviderExtensions
    {

        /// <summary>
        /// Add a folder resource
        /// </summary>
        /// <param name="explorer"></param>
        /// <param name="folder"></param>
        /// <param name="includeSubFolders"></param>
        /// <param name="monitorChanges"></param>
        /// <returns></returns>
        public static ResourceExplorer AddFolder(this ResourceExplorer explorer, string folder, bool includeSubFolders = true, bool monitorChanges = true)
        {
            explorer.AddResourceProvider(new FolderResourceProvider(folder, includeSubFolders: includeSubFolders, monitorChanges: monitorChanges));
            return explorer;
        }

        /// <summary>
        ///  Add folder resources
        /// </summary>
        /// <param name="explorer"></param>
        /// <param name="folder"></param>
        /// <param name="ignoreFolders"></param>
        /// <param name="monitorChanges"></param>
        /// <returns></returns>
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
