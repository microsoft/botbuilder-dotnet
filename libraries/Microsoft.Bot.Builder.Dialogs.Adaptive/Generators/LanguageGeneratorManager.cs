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
        public ConcurrentDictionary<string, LanguageGenerator> LanguageGenerators { get; set; } = new ConcurrentDictionary<string, LanguageGenerator>(StringComparer.OrdinalIgnoreCase);

        public static ImportResolverDelegate ResourceExplorerResolver(string locale, Dictionary<string, IList<Resource>> resourceMapping)
        {
            return (string source, string id) =>
            {
                var fallbackLocale = LGResourceLoader.FallbackLocale(locale, resourceMapping.Keys.ToList());
                var resources = resourceMapping[fallbackLocale];

                var resourceName = Path.GetFileName(PathUtils.NormalizePath(id));

                var resource = resources.FirstOrDefault(u => LGResourceLoader.ParseLGFileName(u.Id).prefix.ToLower() == LGResourceLoader.ParseLGFileName(resourceName).prefix.ToLower());
                if (resource == null)
                {
                    throw new Exception($"There is no matching LG resource for {resourceName}");
                }
                else
                {
                    var content = resource.ReadTextAsync().GetAwaiter().GetResult();
                    return (content, resource.Id);
                }
            };
        }

        private void ResourceExplorer_Changed(object sender, IEnumerable<Resource> resources)
        {
            // reload changed LG files
            foreach (var resource in resources.Where(r => Path.GetExtension(r.Id).ToLower() == ".lg"))
            {
                LanguageGenerators[resource.Id] = GetTemplateEngineLanguageGenerator(resource);
            }
        }

        private TemplateEngineLanguageGenerator GetTemplateEngineLanguageGenerator(Resource resource)
        {
            var fileResource = resource as FileResource;
            if (fileResource == null)
            {
                return new TemplateEngineLanguageGenerator(resource.ReadTextAsync().GetAwaiter().GetResult(), resource.Id, multilanguageResources);
            }
            else
            {
                return new TemplateEngineLanguageGenerator(fileResource.FullName, multilanguageResources);
            }
        }
    }
}
