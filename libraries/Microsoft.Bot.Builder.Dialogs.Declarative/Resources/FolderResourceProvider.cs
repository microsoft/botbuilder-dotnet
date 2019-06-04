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

        public FolderResourceProvider(string folder, bool includeSubFolders=true, bool monitorChanges = true)
        {
            this.IncludeSubFolders = includeSubFolders;
            this.Directory = new DirectoryInfo(folder);
            if (monitorChanges)
            {
                this.Watcher = new FileSystemWatcher(folder);
                this.Watcher.IncludeSubdirectories = this.IncludeSubFolders;
                this.Watcher.EnableRaisingEvents = true;
                this.Watcher.Created += Watcher_Changed;
                this.Watcher.Changed += Watcher_Changed;
                this.Watcher.Deleted += Watcher_Changed;
                this.Watcher.Renamed += Watcher_Renamed;

            }
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (this.Changed != null)
            {
                this.Changed(new IResource[] { new FileResource(e.FullPath)/*, new FileResource(e.OldFullPath) */});
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.Changed != null)
            {
                this.Changed(new IResource[] { new FileResource(e.FullPath) });
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
        /// id -> Resource object)
        /// </summary>
        public IEnumerable<IResource> GetResources(string extension)
        {
            SearchOption option = (this.IncludeSubFolders) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var fileInfo in this.Directory.EnumerateFiles($"*.{extension.TrimStart('.')}", option))
            {
                yield return new FileResource(fileInfo.FullName);
            }
            yield break;
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
