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
using static Microsoft.Bot.Builder.LanguageGeneration.TemplateEngine;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Language generator for TemplateEngine
    /// </summary>
    public class TemplateEngineLanguageGenerator : ILanguageGenerator
    {
        private const string DEFAULTLABEL = "Unknown";
        private TemplateEngine engine;

        public TemplateEngineLanguageGenerator()
        {
            this.engine = new TemplateEngine();
        }

        /// <summary>
        /// Construct with raw LG text 
        /// </summary>
        /// <param name="lgText">lg template text</param>
        /// <param name="importResolver">template resource loader (id) => templateText</param>
        /// <param name="name">optional label for the source of the templates (used for labeling source of template errors)</param>
        public TemplateEngineLanguageGenerator(string lgText, ImportResolverDelegate importResolver = null, string name = null)
        {
            this.Name = name ?? DEFAULTLABEL;
            this.engine = new TemplateEngine().AddText(lgText ?? String.Empty, this.Name, importResolver: importResolver);
        }

        /// <summary>
        /// Construct using prebuilt TemplateEngine
        /// </summary>
        /// <param name="engine">template engine</param>
        /// <param name="name">optional label for the source of the templates (used for labeling source of template errors)</param>
        public TemplateEngineLanguageGenerator(TemplateEngine engine)
        {
            this.engine = engine;
        }

        /// <summary>
        /// Name of the source of this template (used for labeling errors)
        /// </summary>
        public string Name { get; set; } = String.Empty;

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
