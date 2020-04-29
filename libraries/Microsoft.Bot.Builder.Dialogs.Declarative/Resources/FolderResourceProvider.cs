// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{
    /// <summary>
    /// Class which gives ResourceExplorer access to resources which are stored in file system.
    /// </summary>
    public class FolderResourceProvider : ResourceProvider, IDisposable
    {
        private Dictionary<string, FileResource> resources = new Dictionary<string, FileResource>();
        private FileSystemWatcher watcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderResourceProvider"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer.</param>
        /// <param name="folder">Folder.</param>
        /// <param name="includeSubFolders">Should include sub folders.</param>
        /// <param name="monitorChanges">Should monitor changes.</param>
        public FolderResourceProvider(ResourceExplorer resourceExplorer, string folder, bool includeSubFolders = true, bool monitorChanges = true)
            : base(resourceExplorer)
        {
            this.IncludeSubFolders = includeSubFolders;
            folder = PathUtils.NormalizePath(folder);
            this.Directory = new DirectoryInfo(folder);
            this.Id = this.Directory.FullName;
            Refresh();

            if (monitorChanges)
            {
                this.watcher = new FileSystemWatcher(folder);
                this.watcher.IncludeSubdirectories = this.IncludeSubFolders;
                this.watcher.EnableRaisingEvents = true;
                this.watcher.Created += Watcher_Changed;
                this.watcher.Changed += Watcher_Changed;
                this.watcher.Deleted += Watcher_Deleted;
                this.watcher.Renamed += Watcher_Renamed;
            }
        }

        /// <summary>
        /// Gets or sets folder to enumerate.
        /// </summary>
        /// <value>
        /// folder to enumerate.
        /// </value>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include subfolders.
        /// </summary>
        /// <value>
        /// A value indicating whether to include subfolders.
        /// </value>
        public bool IncludeSubFolders { get; set; }

        public void Dispose()
        {
            lock (Directory)
            {
                if (this.watcher != null)
                {
                    this.watcher.EnableRaisingEvents = false;
                    this.watcher.Dispose();
                    this.watcher = null;
                }
            }
        }

        /// <summary>
        /// Refresh any cached content and look for new content.
        /// </summary>
        public override void Refresh()
        {
            this.resources.Clear();

            SearchOption option = this.IncludeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            foreach (var fileInfo in this.Directory.EnumerateFiles($"*.*", option).Where(fi => this.ResourceExplorer.ResourceTypes.Contains(fi.Extension.TrimStart('.'))))
            {
                var fileResource = new FileResource(fileInfo.FullName);
                this.resources[fileResource.Id] = fileResource;
            }
        }

        /// <summary>
        /// GetResource by id.
        /// </summary>
        /// <param name="id">Resource ID.</param>
        /// <param name="resource">the found resource.</param>
        /// <returns>true if resource was found.</returns>
        public override bool TryGetResource(string id, out Resource resource)
        {
            lock (this.resources)
            {
                if (this.resources.TryGetValue(id, out FileResource fileResource))
                {
                    resource = fileResource;
                    return true;
                }

                resource = null;
                return false;
            }
        }

        /// <summary>
        /// Get Resources by extension.
        /// </summary>
        /// <param name="extension">Resource extension.</param>
        /// <returns>Collection of resources.</returns>
        public override IEnumerable<Resource> GetResources(string extension)
        {
            extension = $".{extension.TrimStart('.').ToLower()}";

            lock (this.resources)
            {
                return this.resources.Where(pair => pair.Key.ToLower().EndsWith(extension)).Select(pair => pair.Value).ToList();
            }
        }

        public override string ToString()
        {
            return this.Id;
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            var ext = Path.GetExtension(e.FullPath);
            if (this.ResourceExplorer.ResourceTypes.Contains(ext.TrimStart('.')))
            {
                lock (this.resources)
                {
                    this.resources.Remove(Path.GetFileName(e.FullPath));
                    this.OnChanged(new Resource[] { new FileResource(e.FullPath) });
                }
            }
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            var ext = Path.GetExtension(e.FullPath);
            if (this.ResourceExplorer.ResourceTypes.Contains(ext.TrimStart('.')))
            {
                lock (this.resources)
                {
                    var fileResource = new FileResource(e.FullPath);
                    this.resources[fileResource.Id] = fileResource;
                    this.OnChanged(new Resource[] { fileResource });
                }
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            var ext = Path.GetExtension(e.FullPath);
            if (this.ResourceExplorer.ResourceTypes.Contains(ext.TrimStart('.')))
            {
                var fileResource = new FileResource(e.FullPath);

                lock (this.resources)
                {
                    this.resources[fileResource.Id] = fileResource;
                }

                this.OnChanged(new Resource[] { fileResource });
            }
        }
    }
}
