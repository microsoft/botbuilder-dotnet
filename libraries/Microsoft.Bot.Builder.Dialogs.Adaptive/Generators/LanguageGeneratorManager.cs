// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json.Linq;

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
            RegisterTemplateFunctions();
        }

        /// <summary>
        /// Gets or sets TemplatesMapping from a resourceName and locale pair.
        /// </summary>
        /// <value>
        /// TemplatesMapping of a resourceName and locale pair.
        /// </value>
#pragma warning disable CA2227 // Collection properties should be read only
        public static ConcurrentDictionary<(string resourceName, string locale), LanguageGeneration.Templates> TemplatesMapping { get; set; } = new ConcurrentDictionary<(string resourceId, string locale), LanguageGeneration.Templates>();
#pragma warning restore CA2227 // Collection properties should be read only

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

        private void RegisterTemplateFunctions()
        {
            var allTemplateNames = new List<string>();
            TemplatesMapping.Values.ToList().ForEach(u => allTemplateNames.AddRange(u.AllTemplates.Select(t => t.Name)));
            allTemplateNames = allTemplateNames.Distinct().ToList();

            foreach (var templateName in allTemplateNames)
            {
                Expression.Functions.Add(templateName, new ExpressionEvaluator(
                    templateName,
                    (expression, state, options) =>
                    {
                        object result = null;
                        string error = null;
                        IReadOnlyList<object> args;
                        var locale = options.Locale;
                        var getGenerator = state.TryGetValue(AdaptiveDialog.GeneratorIdKey, out var resourceId);
                        if (getGenerator)
                        {
                            var (resourceName, _) = LGResourceLoader.ParseLGFileName(resourceId.ToString());

                            var getLanguagePolicy = state.TryGetValue(AdaptiveDialog.LanguagePolicy, out var lp);
                            LanguagePolicy languagePolicy;
                            if (!getLanguagePolicy)
                            {
                                languagePolicy = new LanguagePolicy();
                            }
                            else
                            {
                                languagePolicy = JObject.FromObject(lp).ToObject<LanguagePolicy>();
                            }

                            var fallbackLocales = new List<string>();

                            if (languagePolicy.ContainsKey(locale))
                            {
                                fallbackLocales.AddRange(languagePolicy[locale]);
                            }

                            // append empty as fallback to end
                            if (locale.Length != 0 && languagePolicy.ContainsKey(string.Empty))
                            {
                                fallbackLocales.AddRange(languagePolicy[string.Empty]);
                            }

                            if (fallbackLocales.Count == 0)
                            {
                                throw new InvalidOperationException($"No supported language found for {locale}");
                            }

                            foreach (var fallbackLocale in fallbackLocales)
                            {
                                var templatesExist = TemplatesMapping.TryGetValue((resourceName, fallbackLocale.ToLowerInvariant()), out var templates);
                                if (!templatesExist)
                                {
                                    continue;
                                }

                                if (!templates.Exists(u => u.Name == templateName))
                                {
                                    continue;
                                }

                                (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
                                if (error == null)
                                {
                                    var parameters = templates.First(u => u.Name == templateName).Parameters;
                                    var newScope = parameters.Zip(args, (k, v) => new { k, v })
                                        .ToDictionary(x => x.k, x => x.v);
                                    var scope = new StackedMemory();
                                    scope.Push(state);
                                    scope.Push(new SimpleObjectMemory(newScope));
                                    var lgOpt = new EvaluationOptions() { Locale = locale, NullSubstitution = options.NullSubstitution };
                                    result = templates.Evaluate(templateName, scope, lgOpt);
                                    return (result, error);
                                }
                            }
                        }

                        return (result, error);
                    }));
            }
        }
    }
}
