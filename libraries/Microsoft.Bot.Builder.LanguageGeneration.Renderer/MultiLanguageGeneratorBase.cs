using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public abstract class MultiLanguageGeneratorBase : ILanguageGenerator
    {

        public MultiLanguageGeneratorBase()
        {
        }

        public abstract bool TryGetGenerator(ITurnContext context, string locale, out ILanguageGenerator generator);

        /// <summary>
        /// This allows you to specify per language the fallback policies you want
        /// </summary>
        public ILanguagePolicy LanguagePolicy { get; set; } = new LanguagePolicy();

        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            // see if we have any locales that match
            var targetLocale = turnContext.Activity.Locale?.ToLower() ?? string.Empty;

            var locales = new string[] { String.Empty };
            if (!this.LanguagePolicy.TryGetValue(targetLocale, out locales))
            {
                if (!this.LanguagePolicy.TryGetValue(String.Empty, out locales))
                {
                    throw new Exception($"No supported language found for {targetLocale}");
                }
            }

            List<string> errors = new List<string>();
            foreach (var locale in locales)
            {
                if (this.TryGetGenerator(turnContext, locale, out ILanguageGenerator generator))
                {
                    try
                    {
                        return await generator.Generate(turnContext, template, data);
                    }
                    catch(Exception err)
                    {
                        errors.Add(err.Message);
                    }
                }
            }
            throw new Exception(String.Join(",\n", errors.Distinct()));
        }
    }
}
