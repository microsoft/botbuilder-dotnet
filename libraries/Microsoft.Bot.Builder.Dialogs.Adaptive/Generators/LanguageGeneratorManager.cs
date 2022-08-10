// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// Class which manages cache of all LG resources from a ResourceExplorer. 
    /// This class automatically updates the cache when resource change events occure.
    /// </summary>
    public class LanguageGeneratorManager
    {
        /// <summary>
        /// Exports Regex in LG file. Reference: https://docs.microsoft.com/en-us/azure/bot-service/file-format/bot-builder-lg-file-format?view=azure-bot-service-4.0#exports-option.
        /// </summary>
        private static readonly Regex ExportOptionRegex = new Regex(@"\s*>\s*!#\s*@exports\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly ResourceExplorer _resourceExplorer;

        /// <summary>
        /// multi language lg resources. en -> [resourcelist].
        /// </summary>
        private readonly ConcurrentDictionary<string, IList<Resource>> _multilanguageResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageGeneratorManager"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to manage LG files from.</param>
        public LanguageGeneratorManager(ResourceExplorer resourceExplorer)
        {
            _resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));
            _multilanguageResources = new ConcurrentDictionary<string, IList<Resource>>(LGResourceLoader.GroupByLocale(resourceExplorer));

            PopulateLanguageGenerators();

            // listen for resource changes
            _resourceExplorer.Changed += ResourceExplorer_Changed;
        }

        /// <summary>
        /// Gets the language generators.
        /// </summary>
        /// <value>
        /// Generators.
        /// </value>
        public ConcurrentDictionary<string, Lazy<LanguageGenerator>> LanguageGenerators { get; } = new ConcurrentDictionary<string, Lazy<LanguageGenerator>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the resolver to resolve LG import id to template text based on language and a template resource loader delegate.
        /// </summary>
        /// <param name="locale">Locale to identify language.</param>
        /// <param name="resourceMapping">Template resource loader delegate.</param>
        /// <returns>The delegate to resolve the resource.</returns>
        public static ImportResolverDelegate ResourceExplorerResolver(string locale, IDictionary<string, IList<Resource>> resourceMapping)
        {
            return (LGResource lgResource, string id) =>
            {
                var fallbackLocale = LGResourceLoader.FallbackLocale(locale, resourceMapping.Keys.ToList());
                var resources = resourceMapping[fallbackLocale];

                var resourceName = Path.GetFileName(PathUtils.NormalizePath(id));

                var resource = resources.FirstOrDefault(u => LGResourceLoader.ParseLGFileName(u.Id).prefix.ToLowerInvariant() == LGResourceLoader.ParseLGFileName(resourceName).prefix.ToLowerInvariant());
                if (resource == null)
                {
                    throw new InvalidOperationException($"There is no matching LG resource for {resourceName}");
                }

                var content = resource.ReadTextAsync().GetAwaiter().GetResult();
                return new LGResource(resource.Id, resource.FullName, content);
            };
        }

        /// <summary>
        /// Populates the <see cref="LanguageGenerators"/> property with <see cref="Lazy{LaguageGenerator}" /> instances.
        /// </summary>
        /// <remarks>
        /// If the resource contains exports, this method also ensure the LanguageGenerator instance is loaded and ready to use.
        /// </remarks>
        private void PopulateLanguageGenerators()
        {
            var resources = _resourceExplorer.GetResources("lg");

            // Create one LanguageGenerator for each resource.
            foreach (var resource in resources)
            {
                LanguageGenerators[resource.Id] = new Lazy<LanguageGenerator>(() =>
                {
                    // Creates the generator when requested and loads it. 
                    var generator = new TemplateEngineLanguageGenerator(resource, _multilanguageResources);
                    generator.LoadAsync().GetAwaiter().GetResult();
                    return generator;
                });

                // Check if the file contains exports.
                if (ContainsExport(resource))
                {
                    // Force lazy creation for lg files that contain exports
                    // Exports need to be available globally and need to be parsed at startup
                    _ = LanguageGenerators[resource.Id].Value;
                }
            }
        }

        private bool ContainsExport(Resource resource)
        {
            var content = resource.ReadTextAsync().GetAwaiter().GetResult();
            return ExportOptionRegex.IsMatch(content);
        }

        /// <summary>
        /// Loads the specified set of resources by creating template language generators and loading
        /// the content asynchronously. 
        /// </summary>
        /// <remarks>Once LoadAsync() is completed, the manager is fully initialized. Note that the this set up supports
        /// GenerateASync() being called in the generators in other threads while we are loading, and the thread-safe lazy initialization
        /// is ensuring that a single thread initializes the resources.</remarks>
        /// <param name="resources">Resources to load.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task LoadAsync(IEnumerable<Resource> resources)
        {
            var loadTaskMap = new Dictionary<Resource, (Task LoadTask, TemplateEngineLanguageGenerator Generator)>();

            // Create one LanguageGenerator for each resource. Given that each language generator needs
            // to asynchronous load its content, we trigger these loading tasks async and them wait for them all to be done.
            foreach (var resource in resources)
            {
                // Create generator, explicitly asking to not load resources on construction so we can do it asynchronously.
                var generator = new TemplateEngineLanguageGenerator(resource, _multilanguageResources);

                // Capture loading task and store in temporary map.
                var generatorLoadTask = generator.LoadAsync();
                loadTaskMap.Add(resource, (generatorLoadTask, generator));
            }

            // Wait for loading to complete.
            await Task.WhenAll(loadTaskMap.Select(entry => entry.Value.LoadTask)).ConfigureAwait(false);

            // Build our language generator table with the fully loaded generators.
            foreach (var entry in loadTaskMap)
            {
                LanguageGenerators[entry.Key.Id] = new Lazy<LanguageGenerator>(() => entry.Value.Generator);
            }
        }

        private void ResourceExplorer_Changed(object sender, IEnumerable<Resource> resources)
        {
            // reload changed LG files
            LoadAsync(resources.Where(r => Path.GetExtension(r.Id).ToLowerInvariant() == ".lg")).GetAwaiter().GetResult();
        }
    }
}
