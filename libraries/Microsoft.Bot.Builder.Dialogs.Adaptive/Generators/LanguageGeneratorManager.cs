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
        private static readonly Regex ExportOptionRegex = new Regex(@"^\s*>\s*!#\s*@exports\s*=", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private ResourceExplorer _resourceExplorer;

        /// <summary>
        /// multi language lg resources. en -> [resourcelist].
        /// </summary>
        private readonly Dictionary<string, IList<Resource>> _multilanguageResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageGeneratorManager"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to manage LG files from.</param>
        public LanguageGeneratorManager(ResourceExplorer resourceExplorer)
            : this(resourceExplorer, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageGeneratorManager"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to manage LG files from.</param>
        /// <param name="loadOnConstruction">Whether to load language generation resources on construction.</param>
        internal LanguageGeneratorManager(ResourceExplorer resourceExplorer, bool loadOnConstruction)
        {
            _resourceExplorer = resourceExplorer ?? throw new ArgumentNullException(nameof(resourceExplorer));
            _multilanguageResources = LGResourceLoader.GroupByLocale(resourceExplorer); // new Dictionary<string, IList<Resource>>();

            // Legacy path: legacy constructor calls will load the content synchronously in the constructor as before to 
            // maintain backward compatibility. New path through adaptive runtime will call LoadAsync separately in an asynchronous manner.
            if (loadOnConstruction)
            {
                LoadAsync().GetAwaiter().GetResult();
            }
            else
            {
                LazyLoad();
            }

            // listen for resource changes
            _resourceExplorer.Changed += ResourceExplorer_Changed;
        }

        /// <summary>
        /// Gets or sets generators.
        /// </summary>
        /// <value>
        /// Generators.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public ConcurrentDictionary<string, Lazy<LanguageGenerator>> LanguageGenerators { get; set; } = new ConcurrentDictionary<string, Lazy<LanguageGenerator>>(StringComparer.OrdinalIgnoreCase);
#pragma warning restore CA2227 // Collection properties should be read only

        /// <summary>
        /// Returns the resolver to resolve LG import id to template text based on language and a template resource loader delegate.
        /// </summary>
        /// <param name="locale">Locale to identify language.</param>
        /// <param name="resourceMapping">Template resource loader delegate.</param>
        /// <returns>The delegate to resolve the resource.</returns>
        public static ImportResolverDelegate ResourceExplorerResolver(string locale, Dictionary<string, IList<Resource>> resourceMapping)
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
                else
                {
                    var content = resource.ReadTextAsync().GetAwaiter().GetResult();
                    return new LGResource(resource.Id, resource.FullName, content);
                }
            };
        }

        /// <summary>
        /// Lazy load generator managet.
        /// </summary>
        internal void LazyLoad()
        {
            var resources = _resourceExplorer.GetResources("lg");

            // Create one LanguageGenerator for each resource.
            foreach (var resource in resources)
            {
                LanguageGenerators[resource.Id] = new Lazy<LanguageGenerator>(() =>
                 new TemplateEngineLanguageGenerator(resource, _multilanguageResources));

                // Force lazy creation for lg files that contains exports
                // Exports needs to be available globally and need to be parsed at startup
                if (ContainsExport(resource))
                {
                    _ = LanguageGenerators[resource.Id].Value;
                }
            }
        }

        /// <summary>
        /// Default loading of the generator manager, which is usually done on startup and involves loading all LG files.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task LoadAsync()
        {
            await LoadAsync(_resourceExplorer.GetResources("lg")).ConfigureAwait(false);
        }

        private bool ContainsExport(Resource resource)
        {
            try
            {
                var content = File.ReadAllText(resource?.FullName);
                return ExportOptionRegex.IsMatch(content);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return false;
            }
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
                var generator = new TemplateEngineLanguageGenerator(resource, _multilanguageResources, loadOnConstruction: false);

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
