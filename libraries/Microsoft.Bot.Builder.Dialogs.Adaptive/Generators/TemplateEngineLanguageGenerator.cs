// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// ILanguageGenerator implementation which uses LGFile. 
    /// </summary>
    public class TemplateEngineLanguageGenerator : ILanguageGenerator
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.TemplateEngineLanguageGenerator";

        private const string DEFAULTLABEL = "Unknown";

        private readonly Dictionary<string, LGFile> multiLanglgFiles = new Dictionary<string, LGFile>();

        private LGFile lgFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        public TemplateEngineLanguageGenerator()
        {
            this.lgFile = new LGFile();
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
            var (_, locale) = MultiLanguageResourceLoader.ParseLGFileName(id);
            var fallbackLocale = MultiLanguageResourceLoader.FallbackLocale(locale, resourceMapping.Keys.ToList());

            foreach (var mapping in resourceMapping)    
            {
                // if no locale present in id, enumarate every locale found
                // if locale is present, use that one
                if (string.Equals(fallbackLocale, string.Empty) || string.Equals(fallbackLocale, mapping.Key))
                {
                    var engine = LGParser.ParseText(lgText ?? string.Empty, Id, LanguageGeneratorManager.ResourceExplorerResolver(mapping.Key, resourceMapping));
                    multiLanglgFiles.Add(mapping.Key, engine);
                }
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

            var (_, locale) = MultiLanguageResourceLoader.ParseLGFileName(Id);
            var fallbackLocale = MultiLanguageResourceLoader.FallbackLocale(locale, resourceMapping.Keys.ToList());

            foreach (var mapping in resourceMapping)
            {
                // if no locale present in id, enumarate every locale found
                // if locale is present, use that one
                if (string.Equals(fallbackLocale, string.Empty) || string.Equals(fallbackLocale, mapping.Key))
                {
                    var lgFile = LGParser.ParseFile(filePath, LanguageGeneratorManager.ResourceExplorerResolver(mapping.Key, resourceMapping));
                    multiLanglgFiles.Add(mapping.Key, lgFile);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="lgFile">template engine.</param>
        public TemplateEngineLanguageGenerator(LGFile lgFile)
        {
            this.lgFile = lgFile;
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
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>generated text.</returns>
        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            lgFile = InitLGFile(turnContext);

            try
            {
                return await Task.FromResult(lgFile.Evaluate(template, data).ToString());
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

        private LGFile InitLGFile(ITurnContext turnContext)
        {
            var locale = turnContext.Activity.Locale?.ToLower() ?? string.Empty;
            if (multiLanglgFiles.Count > 0)
            {
                var fallbackLocale = MultiLanguageResourceLoader.FallbackLocale(locale, multiLanglgFiles.Keys.ToList());
                lgFile = multiLanglgFiles[fallbackLocale];
            }
            else
            {
                // Do not rewrite to ??= (C# 8.0 new feature). It will break in linux/mac
                lgFile = lgFile ?? new LGFile();
            }

            return lgFile;
        }
    }
}
