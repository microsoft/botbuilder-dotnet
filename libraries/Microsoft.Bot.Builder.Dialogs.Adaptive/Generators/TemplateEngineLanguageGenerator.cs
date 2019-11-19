// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator implementation which uses TemplateEngine. 
    /// </summary>
    public class TemplateEngineLanguageGenerator : ILanguageGenerator
    {
        private const string DEFAULTLABEL = "Unknown";

        private readonly Dictionary<string, TemplateEngine> multiLangEngines = new Dictionary<string, TemplateEngine>();

        private TemplateEngine engine;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        public TemplateEngineLanguageGenerator()
        {
            this.engine = new TemplateEngine();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="lgText">lg template text.</param>
        /// <param name="id">optional label for the source of the templates (used for labeling source of template errors).</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(string lgText, string id, Dictionary<string, IList<IResource>> resourceMapping)
        {
            this.Id = id ?? DEFAULTLABEL;
            foreach (var mappingItem in resourceMapping)
            {
                var engine = new TemplateEngine().AddText(lgText ?? string.Empty, Id, LanguageGeneratorManager.ResourceExplorerResolver(mappingItem.Key, resourceMapping));
                multiLangEngines.Add(mappingItem.Key, engine);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="filePath">lg template file absolute path.</param>
        /// <param name="resourceMapping">template resource loader delegate (locale) -> <see cref="ImportResolverDelegate"/>.</param>
        public TemplateEngineLanguageGenerator(string filePath, Dictionary<string, IList<IResource>> resourceMapping)
        {
            filePath = PathUtils.NormalizePath(filePath);
            this.Id = Path.GetFileName(filePath);
            foreach (var mappingItem in resourceMapping)
            {
                var engine = new TemplateEngine().AddFile(filePath, LanguageGeneratorManager.ResourceExplorerResolver(mappingItem.Key, resourceMapping));
                multiLangEngines.Add(mappingItem.Key, engine);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="engine">template engine.</param>
        public TemplateEngineLanguageGenerator(TemplateEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        /// Gets or sets id of the source of this template (used for labeling errors).
        /// </summary>
        /// <value>
        /// Id of the source of this template (used for labeling errors).
        /// </value>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Method to generate text from given template and data.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>generated text.</returns>
        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            engine = InitTemplateEngine(turnContext);

            try
            {
                return await Task.FromResult(engine.Evaluate(template, data).ToString());
            }
            catch (Exception err)
            {
                if (!string.IsNullOrEmpty(this.Id))
                {
                    throw new Exception($"{Id}:{err.Message}");
                }

                throw;
            }
        }

        private TemplateEngine InitTemplateEngine(ITurnContext turnContext)
        {
            var locale = turnContext.Activity.Locale?.ToLower() ?? string.Empty;
            if (multiLangEngines.Count > 0)
            {
                var fallbackLocale = MultiLanguageResourceLoader.FallbackLocale(locale, multiLangEngines.Keys.ToList());
                engine = multiLangEngines[fallbackLocale];
            }
            else
            {
                // Do not rewrite to ??= (C# 8.0 new feature). It will break in linux/mac
                engine = engine ?? new TemplateEngine();
            }

            return engine;
        }
    }
}
