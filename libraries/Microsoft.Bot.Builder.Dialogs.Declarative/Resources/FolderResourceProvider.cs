using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{

    /// <summary>
    /// Folder/FileResources
    /// </summary>
    public class FolderResourceProvider : IBotResourceProvider, IBotResourceWatcher
    {
        private FileSystemWatcher watcher = null;
        private List<IBotResource> resources = new List<IBotResource>();

        public FolderResourceProvider(string folder = null, bool monitorChanges = true)
        {
            this.Id = folder;
            this.Folder = folder;
            this.MonitorChanges = monitorChanges;
        }

        /// <summary>
        /// Id of the source
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// folder to enumerate
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// id -> Resource object)
        /// </summary>
        public async Task<IBotResource[]> GetResources(string resourceType)
        {
            await this.loadResourcesFromFolder();

            return this.resources.Where(r => r.ResourceType == resourceType).ToArray();
        }

        public bool MonitorChanges { get; set; }

        public event ResourceChangeHandler Changed;

        private Task loadResourcesFromFolder()
        {
            lock (Folder)
            {
                if (this.resources.Count == 0 || this.MonitorChanges == false)
                {
                    var dir = new DirectoryInfo(this.Folder);
                    foreach (var fileInfo in dir.EnumerateFiles("*.*", SearchOption.AllDirectories))
                    {
                        if (fileInfo.Extension.Length > 0)
                        {
                            var resourceType = fileInfo.Extension.Substring(1).ToLower();
                            if (BotResourceManager.ResourceTypes.Contains(resourceType))
                            {
                                this.resources.Add(new FileResource()
                                {
                                    Id = fileInfo.FullName,
                                    Source = this,
                                    Name = Path.GetFileNameWithoutExtension(fileInfo.Name),
                                    Path = fileInfo.FullName,
                                    ResourceType = resourceType
                                });
                            }
                        }
                    }
                }

                if (MonitorChanges && this.watcher == null)
                {
                    this.watcher = new FileSystemWatcher(this.Folder);
                    this.watcher.IncludeSubdirectories = true;
                    this.watcher.Changed += Watcher_Changed;
                    this.watcher.Renamed += Watcher_Renamed;
                    // this.watcher.Created += Watcher_Created;
                    // this.watcher.Deleted += Watcher_Deleted;
                    this.watcher.EnableRaisingEvents = true;
                }
            }
            return Task.CompletedTask;
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (this.Changed != null)
            {
                var resource = this.resources.Cast<FileResource>().SingleOrDefault(r => Path.Equals(r.Path, e.OldFullPath));
                if (resource != null)
                {
                    this.resources.Remove(resource);

                    var newResource = AddNewResource(e.FullPath);
                    if (newResource != null)
                    {
                        this.Changed(this, newResource);
                    }
                }
            }
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (this.Changed != null)
            {
                var resource = this.resources.Cast<FileResource>().SingleOrDefault(r => Path.Equals(r.Path, e.FullPath));
                if (resource == null)
                {
                    var extension = Path.GetExtension(e.Name);

                    resource = new FileResource()
                    {
                        Id = e.FullPath,
                        Source = this,
                        Name = Path.GetFileNameWithoutExtension(e.Name),
                        Path = e.FullPath,
                        ResourceType = extension.Length > 0 ? extension.Substring(1) : string.Empty
                    };
                    this.resources.Add(resource);
                }
                this.Changed(this, resource);
            }
        }

        //private void Watcher_Created(object sender, FileSystemEventArgs e)
        //{
        //    if (this.Changed != null)
        //    {
        //        var resource = AddNewResource(e.FullPath);
        //        if (resource != null)
        //        {
        //            this.Changed(this, resource);
        //        }
        //    }
        //}

        private IBotResource AddNewResource(string path)
        {
            var resourceType = Path.GetExtension(path).ToLower().Substring(1);
            if (BotResourceManager.ResourceTypes.Contains(resourceType))
            {
                var resource = new FileResource()
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    ResourceType = resourceType
                };
                this.resources.Add(resource);
                return resource;
            }
            return null;
        }
    }
}
