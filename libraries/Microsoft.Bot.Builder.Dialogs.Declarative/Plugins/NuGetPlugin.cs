using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Loaders;
using Microsoft.Bot.Builder.Dialogs.Declarative.Types;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Microsoft.Bot.Builder.Dialogs.Declarative.Plugins
{
    public class NugetPlugin : IPlugin
    {
        private readonly NuGetDependencyInfo info;

        public NugetPlugin(NuGetDependencyInfo info)
        {
            this.info = info ?? throw new ArgumentNullException(nameof(info));
        }

        public string SchemaUri { get; private set; }
        public Type Type { get; private set; }
        public ICustomDeserializer Loader { get; private set; }

        public async Task Load()
        {
            // NuGet providers
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support

            // Package source. Example: https://api.nuget.org/v3/index.json
            var packageSource = new PackageSource(info.PackageSource);
            var sourceRepository = new SourceRepository(packageSource, providers);

            // Operation to download package
            var downloadResource = sourceRepository.GetResource<DownloadResource>();
            var settings = Settings.LoadDefaultSettings(root: null);

            // Download the package to the NuGet default directory
            var result = await downloadResource.GetDownloadResourceResultAsync(
                    new PackageIdentity(info.PackageName, new NuGetVersion(info.PackageVersion)),
                    new PackageDownloadContext(
                            new SourceCacheContext()),
                    SettingsUtility.GetGlobalPackagesFolder(settings),
                    NullLogger.Instance,
                    CancellationToken.None);

            var packagePathResolver = new PackagePathResolver(Path.GetFullPath("packages"));
            var packageExtractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3,
                XmlDocFileSaveMode.None,
                ClientPolicyContext.GetClientPolicy(settings, NullLogger.Instance),
                NullLogger.Instance);

            // Extract compressed nupkg file
            var extractResult = await PackageExtractor.ExtractPackageAsync(
                    result.PackageSource,
                    result.PackageStream,
                    packagePathResolver,
                    packageExtractionContext,
                    CancellationToken.None);

            // Find closest package to requested runtime version
            var frameworkReducer = new FrameworkReducer();
            var nuGetframework = NuGetFramework.ParseFolder(info.RuntimeVersion);
            var libItems = result.PackageReader.GetLibItems();

            var nearest = frameworkReducer.GetNearest(nuGetframework, libItems.Select(x => x.TargetFramework));

            // Obtain files for the right package
            var packageFile = libItems
                .Where(x => x.TargetFramework.Equals(nearest))
                .SelectMany(x => x.Items)
                .FirstOrDefault(x => x.Contains(info.AssemblyFile));

            // Load assembly for factory registration and custom loader if applicable
            var assembly = Assembly.LoadFrom(packageFile);

            this.Type = assembly.GetTypes().FirstOrDefault(t => t.Name == info.ClassName);
            this.SchemaUri = info.Schema;

            if (!string.IsNullOrEmpty(info.LoaderClassName))
            {
                this.Loader = Activator.CreateInstance(assembly.GetType(info.LoaderClassName)) as ICustomDeserializer;
            }
        }
    }
}
