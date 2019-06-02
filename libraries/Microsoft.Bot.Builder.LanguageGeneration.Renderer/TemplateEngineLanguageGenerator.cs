using System;
using System.Collections.Generic;
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

        public TemplateEngineLanguageGenerator(string lgText=null)
        {
            this.engine = TemplateEngine.FromText(lgText ?? String.Empty);
        }

        public TemplateEngineLanguageGenerator(TemplateEngine engine)
        {
            this.engine = engine;
        }

        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            return engine.Evaluate(template, data);
        }
    }
}
