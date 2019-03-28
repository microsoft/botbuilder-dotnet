using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Microsoft.Bot.Builder.Dialogs.Declarative
{
    /// <summary>
    /// Class which gives standard access to file based resources
    /// </summary>
    public class ResourceExplorer : IResourceExplorer
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

        IEnumerable<DirectoryInfo> IResourceExplorer.Folders { get => folderResources.Select(s => s.Directory); set => throw new NotImplementedException(); }

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is deleted.
        /// </summary>
        public event FileSystemEventHandler Deleted;

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is created.
        /// </summary>
        public event FileSystemEventHandler Created;

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is changed.
        /// </summary>
        public event FileSystemEventHandler Changed;

        /// <summary>
        /// Occurs when a file or directory in the specified System.IO.FileSystemWatcher.Path is renamed.
        /// </summary>
        public event RenamedEventHandler Renamed;

        public void AddFolderResource(string folder, bool monitorFiles = true)
        {
            var folderResource = new FolderResource(folder, monitorFiles);

            //folderResource.Watcher.Created += (sender, e) =>
            //{
            //    if (this.Created != null)
            //    {
            //        this.Created(sender, e);
            //    }
            //};

            folderResource.Watcher.Changed += (sender, e) =>
            {
                if (this.Changed != null)
                {
                    this.Changed(sender, e);
                }
            };

            folderResource.Watcher.Deleted += (sender, e) =>
            {
                if (this.Deleted != null)
                {
                    this.Deleted(sender, e);
                }
            };

            //folderResource.Watcher.Renamed += (sender, e) =>
            //{
            //    if (this.Renamed != null)
            //    {
            //        this.Renamed(sender, e);
            //    }
            //};

            this.folderResources.Add(folderResource);
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
            explorer.AddFolderResource(projectFolder);

            // add project references
            foreach (XmlNode node in xmlDoc.SelectNodes("//ProjectReference"))
            {
                var path = Path.Combine(projectFolder, node.Attributes["Include"].Value);
                path = Path.GetFullPath(path);
                path = Path.GetDirectoryName(path);
                explorer.AddFolderResource(path);
            }

            var packages = Path.GetFullPath("packages");
            while (!Directory.Exists(packages) && Path.GetDirectoryName(packages) != Path.GetPathRoot(packages))
            {
                packages = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(packages), @"..\packages"));
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
                        explorer.AddFolderResource(folder, monitorFiles: false);
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
        /// Folder/FileResources
        /// </summary>
        internal class FolderResource
        {
            internal FolderResource(string folder, bool monitorChanges = true)
            {
                this.Directory = new DirectoryInfo(folder);
                this.Watcher = new FileSystemWatcher(folder);
                if (monitorChanges)
                {
                    this.Watcher.IncludeSubdirectories = true;
                    this.Watcher.EnableRaisingEvents = true;
                }
            }

            /// <summary>
            /// folder to enumerate
            /// </summary>
            public DirectoryInfo Directory { get; set; }

            public FileSystemWatcher Watcher { get; private set; }

            /// <summary>
            /// id -> Resource object)
            /// </summary>
            public IEnumerable<FileInfo> GetResources(string extension)
            {
                foreach (var fileInfo in this.Directory.EnumerateFiles($"*.{extension}", SearchOption.AllDirectories))
                {
                    yield return fileInfo;
                }
                yield break;
            }
        }


    }
}
