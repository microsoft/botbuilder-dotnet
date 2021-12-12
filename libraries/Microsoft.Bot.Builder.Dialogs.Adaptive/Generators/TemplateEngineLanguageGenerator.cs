// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.Dialogs.Debugging;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator implementation which uses LGFile. 
    /// </summary>
    public class TemplateEngineLanguageGenerator : LanguageGenerator
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TemplateEngineLanguageGenerator";

        private const string DEFAULTLABEL = "Unknown";

        private readonly Lazy<Task<LanguageGeneration.Templates>> _lg;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        public TemplateEngineLanguageGenerator()
        {
            _lg = new Lazy<Task<LanguageGeneration.Templates>>(() => Task.FromResult(new LanguageGeneration.Templates()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="engine">template engine.</param>
        public TemplateEngineLanguageGenerator(LanguageGeneration.Templates engine = null)
        {
            _lg = new Lazy<Task<LanguageGeneration.Templates>>(() => Task.FromResult(engine ?? new LanguageGeneration.Templates()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="lgText">lg template text.</param>
        /// <param name="id">optional label for the source of the templates (used for labeling source of template errors).</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        [Obsolete("This method will soon be deprecated. Use LGResource as the first parameter instead.")]
        public TemplateEngineLanguageGenerator(string lgText, string id, Dictionary<string, IList<Resource>> resourceMapping)
        {
            Id = id ?? DEFAULTLABEL;
            var (_, locale) = LGResourceLoader.ParseLGFileName(id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var lgResource = new LGResource(Id, Id, lgText ?? string.Empty);
            _lg = new Lazy<Task<LanguageGeneration.Templates>>(() => Task.FromResult(LanguageGeneration.Templates.ParseResource(lgResource, importResolver)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="filePath">lg template file absolute path.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        [Obsolete("This method will soon be deprecated. Use LGResource as the first parameter instead.")]
        public TemplateEngineLanguageGenerator(string filePath, Dictionary<string, IList<Resource>> resourceMapping)
        {
            filePath = PathUtils.NormalizePath(filePath);
            Id = Path.GetFileName(filePath);

            var (_, locale) = LGResourceLoader.ParseLGFileName(Id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var resource = new LGResource(Id, filePath, File.ReadAllText(filePath));
            _lg = new Lazy<Task<LanguageGeneration.Templates>>(() => Task.FromResult(LanguageGeneration.Templates.ParseResource(resource, importResolver)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(Resource resource, Dictionary<string, IList<Resource>> resourceMapping)
            : this(resource, resourceMapping, loadOnConstruction: true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        /// <param name="loadOnConstruction">Whether to load LG resources at build time. If false is specified, then LoadAsync needs to be called.</param>
        internal TemplateEngineLanguageGenerator(Resource resource, Dictionary<string, IList<Resource>> resourceMapping, bool loadOnConstruction)
        {
            Id = resource.Id;
            _lg = new Lazy<Task<LanguageGeneration.Templates>>(() => CreateTemplatesAsync(resource, resourceMapping));

            if (loadOnConstruction)
            {
                // Legacy constructor forces lazy loading for backward compatible behavior.
                _ = _lg.Value.GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Gets or sets id of the source of this template (used for labeling errors).
        /// </summary>
        /// <value>
        /// Id of the source of this template (used for labeling errors).
        /// </value>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Method to generate text from given template and data.
        /// </summary>
        /// <param name="dialogContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>generated text.</returns>
        public override async Task<object> GenerateAsync(DialogContext dialogContext, string template, object data, CancellationToken cancellationToken = default)
        {
            var lgOpt = new EvaluationOptions() { Locale = dialogContext.GetLocale() };

            try
            {
                var lg = await _lg.Value.ConfigureAwait(false);
                return lg.EvaluateText(template, data, lgOpt);
            }
            catch (Exception err)
            {
                if (!string.IsNullOrEmpty(this.Id))
                {
                    throw new InvalidOperationException($"{Id}:{err.Message}");
                }

                throw;
            }
        }

        /// <summary>
        /// Load templates.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task LoadAsync()
        {
            _ = await _lg.Value.ConfigureAwait(false);
        }

        private static void RegisterSourcemap(LanguageGeneration.Templates templates, Resource resource)
        {
            foreach (var template in templates.AllTemplates)
            {
                RegisterSourcemap(template, template.SourceRange, template.SourceRange.Source);
                foreach (var expressionRef in template.Expressions)
                {
                    RegisterSourcemap(expressionRef, expressionRef.SourceRange, resource.FullName);
                }
            }
        }

        private static void RegisterSourcemap(object item, LanguageGeneration.SourceRange sr, string path)
        {
            if (Path.IsPathRooted(path))
            {
                var debugSM = new Debugging.SourceRange(
                    path,
                    sr.Range.Start.Line,
                    sr.Range.Start.Character + 1,
                    sr.Range.End.Line,
                    sr.Range.End.Character + 1);

                if (!DebugSupport.SourceMap.TryGetValue(item, out var _))
                {
                    DebugSupport.SourceMap.Add(item, debugSM);
                }
            }
        }

        /// <summary>
        /// Loads language generation templates asynchronously.
        /// </summary>
        /// <param name="resource">Resource.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        /// <returns>The loaded language generation templates.</returns>
        private async Task<LanguageGeneration.Templates> CreateTemplatesAsync(Resource resource, Dictionary<string, IList<Resource>> resourceMapping)
        {
            var (_, locale) = LGResourceLoader.ParseLGFileName(Id);
            var importResolver = LanguageGeneratorManager.ResourceExplorerResolver(locale, resourceMapping);
            var content = await resource.ReadTextAsync().ConfigureAwait(false);
            var lgResource = new LGResource(Id, resource.FullName, content);
            var lg = LanguageGeneration.Templates.ParseResource(lgResource, importResolver);
            RegisterSourcemap(lg, resource);
            return lg;
        }
    }
}
