using System;
using System.IO;
using System.Threading.Tasks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Resources
{

    /// <summary>
    /// Nuget package resources
    /// </summary>
    public class NugetResourceProvider : IBotResourceProvider
    {
        private PackagePathResolver pathResolver;
        private FolderResourceProvider folderResourceSource;

        public NugetResourceProvider() : this(null, null)
        {
        }

        public NugetResourceProvider(string package, string version)
        {
            pathResolver = new PackagePathResolver(Path.GetFullPath("packages"));
            this.Package = package;
            this.Version = version;
        }

        /// <summary>
        /// Id of the source
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// folder to enumerate
        /// </summary>
        public string Package { get; set; }

        /// <summary>
        /// version of the package
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Resources object
        /// </summary>
        public async Task<IBotResource[]> GetResources(string resourceType)
        {
            await loadResources();

            if (this.folderResourceSource != null)
            {
                return await this.folderResourceSource.GetResources(resourceType);
            }
            return Array.Empty<IBotResource>();
        }

        private async Task loadResources()
        {
            if (this.folderResourceSource == null)
            {
                var packages = Path.GetFullPath("packages");
                while (!Directory.Exists(packages) && packages != @"C:\packages")
                {
                    packages = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(packages), @"..\packages"));
                    if (packages == null)
                    {
                        throw new ArgumentNullException("Can't find packages folder");
                    }
                }
                var package = new PackageIdentity(Package, new NuGetVersion(this.Version));
                var folder = Path.Combine(packages, this.pathResolver.GetPackageDirectoryName(package));
                if (Directory.Exists(folder))
                {
                    this.folderResourceSource = new FolderResourceProvider(folder, monitorChanges: false);
                }
            }
        }
    }
}
