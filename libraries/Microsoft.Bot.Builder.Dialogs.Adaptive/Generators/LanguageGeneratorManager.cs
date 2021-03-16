// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private ResourceExplorer resourceExplorer;

        /// <summary>
        /// multi language lg resources. en -> [resourcelist].
        /// </summary>
        private readonly Dictionary<string, IList<Resource>> multilanguageResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageGeneratorManager"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer to manage LG files from.</param>
        public LanguageGeneratorManager(ResourceExplorer resourceExplorer)
        {
            this.resourceExplorer = resourceExplorer;
            multilanguageResources = LGResourceLoader.GroupByLocale(resourceExplorer);

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
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public ConcurrentDictionary<string, LanguageGenerator> LanguageGenerators { get; set; } = new ConcurrentDictionary<string, LanguageGenerator>(StringComparer.OrdinalIgnoreCase);
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

        private void ResourceExplorer_Changed(object sender, IEnumerable<Resource> resources)
        {
            // reload changed LG files
            foreach (var resource in resources.Where(r => Path.GetExtension(r.Id).ToLowerInvariant() == ".lg"))
            {
                LanguageGenerators[resource.Id] = GetTemplateEngineLanguageGenerator(resource);
            }
        }

        private TemplateEngineLanguageGenerator GetTemplateEngineLanguageGenerator(Resource resource)
        {
            return new TemplateEngineLanguageGenerator(resource, multilanguageResources);
        }
    }
}
