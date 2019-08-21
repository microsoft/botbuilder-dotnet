using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Class which manages cache of all LG resources from a ResourceExplorer. 
    /// This class automatically updates the cache when resource change events occure.
    /// </summary>
    public class LanguageGeneratorManager
    {
        private ResourceExplorer resourceExplorer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageGeneratorManager"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to manage LG files from.</param>
        public LanguageGeneratorManager(ResourceExplorer resourceExplorer)
        {
            this.resourceExplorer = resourceExplorer;
            
            // load all LG resources
            foreach (var resource in this.resourceExplorer.GetResources("lg"))
            {
                LanguageGenerators[resource.Id] = GetTemplateEngineLanguageGenerator(resource);
            }

            // listen for resource changes
            this.resourceExplorer.Changed += ResourceExplorer_Changed;
        }

        /// <summary>
        /// Gets or sets generators.
        /// </summary>
        /// <value>
        /// Generators.
        /// </value>
        public ConcurrentDictionary<string, ILanguageGenerator> LanguageGenerators { get; set; } = new ConcurrentDictionary<string, ILanguageGenerator>(StringComparer.OrdinalIgnoreCase);

        public static ImportResolverDelegate ResourceResolver(ResourceExplorer resourceExplorer) =>
            (string source, string id) =>
            {
                var resourceName = Path.GetFileName(PathUtils.NormalizePath(id));
                var res = resourceExplorer.GetResource(resourceName);

                var content = res?.ReadTextAsync().GetAwaiter().GetResult();

                return (content, resourceName);
            };

        private void ResourceExplorer_Changed(IResource[] resources)
        {
            // reload changed LG files
            foreach (var resource in resources.Where(r => Path.GetExtension(r.Id).ToLower() == ".lg"))
            {
                LanguageGenerators[resource.Id] = GetTemplateEngineLanguageGenerator(resource);
            }
        }

        private TemplateEngineLanguageGenerator GetTemplateEngineLanguageGenerator(IResource resource)
        {
            return new TemplateEngineLanguageGenerator(resource.ReadTextAsync().GetAwaiter().GetResult(), resource.Id, ResourceResolver(resourceExplorer));
        }
    }
}
