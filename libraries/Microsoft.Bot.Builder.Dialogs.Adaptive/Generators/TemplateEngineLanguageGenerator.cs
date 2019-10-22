
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
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
        private TemplateEngine engine;
        private readonly ResourceExplorer resourceExplorer;
        private readonly IResource resource;

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEngineLanguageGenerator"/> class.
        /// </summary>
        /// <param name="resourceExplorer">resourceExplorer.</param>
        /// <param name="resource">resource.</param>
        public TemplateEngineLanguageGenerator(ResourceExplorer resourceExplorer, IResource resource = null)
        {
            this.resourceExplorer = resourceExplorer;
            this.resource = resource;
        }

        /// <summary>
        /// Initializes a new instance of engine.
        /// </summary>
        /// <param name="engine">template engine.</param>
        public void InitEngine(TemplateEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        /// Method to generate text from given template and data.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="template">template to evaluate.</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>generated text.</returns>
        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            var id = DEFAULTLABEL;
            if (resource != null)
            {
                var content = await resource.ReadTextAsync();
                id = resource.Id;

                this.engine = new TemplateEngine().AddText(content, id, ResourceResolver(turnContext, resourceExplorer));
            }
            else
            {
                this.engine ??= new TemplateEngine();
            }

            try
            {
                return await Task.FromResult(engine.Evaluate(template, data).ToString());
            }
            catch (Exception err)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    throw new Exception($"{id}:{err.Message}");
                }

                throw;
            }
        }

        private ImportResolverDelegate ResourceResolver(ITurnContext turnContext, ResourceExplorer resourceExplorer) =>
           (string source, string id) =>
           {
               var languagePolicy = new LanguagePolicy();
               var targetLocale = turnContext.Activity.Locale?.ToLower() ?? string.Empty;

               var locales = new string[] { string.Empty };
               if (!languagePolicy.TryGetValue(targetLocale, out locales))
               {
                   if (!languagePolicy.TryGetValue(string.Empty, out locales))
                   {
                       throw new Exception($"No supported language found for {targetLocale}");
                   }
               }

               var resourceName = Path.GetFileName(PathUtils.NormalizePath(id));

               foreach (var locale in locales)
               {
                   var resourceId = string.IsNullOrEmpty(locale) ? resourceName : resourceName.Replace(".lg", $".{locale}.lg");

                   if (resourceExplorer.TryGetResource(resourceId, out var resource))
                   {
                       var content = resource.ReadTextAsync().GetAwaiter().GetResult();

                       return (content, resourceName);
                   }
               }

               return (string.Empty, resourceName);
           };
    }
}
