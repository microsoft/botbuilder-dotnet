using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Language generator for TemplateEngine
    /// </summary>
    public class TemplateEngineLanguageGenerator : ILanguageGenerator
    {
        private const string DEFAULTLABEL = "Unknown";
        private TemplateEngine engine;
        private Func<string, string> resourceLoader = (id) => string.Empty;

        public TemplateEngineLanguageGenerator()
        {
            this.engine = TemplateEngine.FromText("");
        }

        /// <summary>
        /// Construct with raw LG text 
        /// </summary>
        /// <param name="lgText">lg template text</param>
        /// <param name="resourceLoader">template resource loader (id) => templateText</param>
        /// <param name="name">optional label for the source of the templates (used for labeling source of template errors)</param>
        public TemplateEngineLanguageGenerator(string lgText, Func<string, string> resourceLoader = null, string name = null)
        {
            this.Name = name ?? DEFAULTLABEL;
            this.engine = TemplateEngine.FromText(lgText ?? String.Empty);
            if (resourceLoader != null)
            {
                this.resourceLoader = resourceLoader;
            }
        }

        /// <summary>
        /// Construct using prebuilt TemplateEngine
        /// </summary>
        /// <param name="engine">template engine</param>
        /// <param name="resourceLoader">template resource loader (resourceId) => templateText</param>
        /// <param name="name">optional label for the source of the templates (used for labeling source of template errors)</param>
        public TemplateEngineLanguageGenerator(TemplateEngine engine, Func<string, string> resourceLoader = null, string name = null)
        {
            this.Name = name ?? DEFAULTLABEL;
            this.engine = engine;
            if (resourceLoader != null)
            {
                this.resourceLoader = resourceLoader;
            }
        }

        /// <summary>
        /// Name of the source of this template (used for labeling errors)
        /// </summary>
        public string Name { get; set; }

        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            try
            {
                return engine.Evaluate(template, data);
            }
            catch (Exception err)
            {
                if (!String.IsNullOrEmpty(this.Name))
                {
                    throw new Exception($"{Name}:{err.Message}");
                }
                throw;
            }
        }
    }
}
