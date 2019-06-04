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
        private TemplateEngine engine;

        public TemplateEngineLanguageGenerator()
        {
            this.engine = TemplateEngine.FromText("");
        }

        public TemplateEngineLanguageGenerator(string name, string lgText)
        {
            this.Name = name;
            this.engine = TemplateEngine.FromText(lgText ?? String.Empty);
        }

        public TemplateEngineLanguageGenerator(string name, TemplateEngine engine)
        {
            this.Name = name;
            this.engine = engine;
        }

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
