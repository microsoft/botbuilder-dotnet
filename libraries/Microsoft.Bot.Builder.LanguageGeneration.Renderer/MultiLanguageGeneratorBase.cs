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
        /// This allows you to specify per language the fallback policies you want.
        /// </summary>
        public ILanguagePolicy LanguagePolicy { get; set; } = new LanguagePolicy();

        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            // see if we have any locales that match
            var targetLocale = turnContext.Activity.Locale?.ToLower() ?? string.Empty;

            var locales = new string[] { string.Empty };
            if (!this.LanguagePolicy.TryGetValue(targetLocale, out locales))
            {
                if (!this.LanguagePolicy.TryGetValue(string.Empty, out locales))
                {
                    throw new Exception($"No supported language found for {targetLocale}");
                }
            }

            var generators = new List<ILanguageGenerator>();
            foreach (var locale in locales)
            {
                if (this.TryGetGenerator(turnContext, locale, out ILanguageGenerator generator))
                {
                    generators.Add(generator);
                }
            }

            if (generators.Count == 0)
            {
                throw new Exception($"No generator found for language {targetLocale}");
            }

            var errors = new List<string>();
            foreach (var generator in generators)
            {
                try
                {
                    return await generator.Generate(turnContext, template, data);
                }
                catch (Exception err)
                {
                    errors.Add(err.Message);
                }
            }

            throw new Exception(string.Join(",\n", errors.Distinct()));
        }
    }
}
