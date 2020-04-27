// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// Base class which applies language policy to virtual method of TryGetGenerator.
    /// </summary>
    public abstract class MultiLanguageGeneratorBase : LanguageGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageGeneratorBase"/> class.
        /// </summary>
        public MultiLanguageGeneratorBase()
        {
        }

        [JsonProperty("languagePolicy")]
        public LanguagePolicy LanguagePolicy { get; set; }

        /// <summary>
        /// Abstract method to get an ILanguageGenerator by locale.
        /// </summary>
        /// <param name="dialogContext">context.</param>
        /// <param name="locale">locale.</param>
        /// <param name="generator">generator to return.</param>
        /// <returns>true if found.</returns>
        public abstract bool TryGetGenerator(DialogContext dialogContext, string locale, out LanguageGenerator generator);

        /// <summary>
        /// Find a language generator that matches the current context locale.
        /// </summary>
        /// <param name="dialogContext">Context for the current turn of conversation.</param>
        /// <param name="template">The template.</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>The generator.</returns>
        public override async Task<string> Generate(DialogContext dialogContext, string template, object data)
        {
            var targetLocale = dialogContext.Context.Activity.Locale?.ToLower() ?? string.Empty;

            // priority 
            // 1. local policy
            // 2. shared policy in turnContext
            // 3. default policy
            var languagePolicy = this.LanguagePolicy ?? 
                                dialogContext.Services.Get<LanguagePolicy>() ?? 
                                new LanguagePolicy();

            // see if we have any locales that match
            var fallbackLocales = new List<string>();
            if (languagePolicy.ContainsKey(targetLocale))
            {
                fallbackLocales.AddRange(languagePolicy[targetLocale]);
            }
            
            // append empty as fallback to end
            if (targetLocale != string.Empty && languagePolicy.ContainsKey(string.Empty))
            {
                fallbackLocales.AddRange(languagePolicy[string.Empty]);
            }

            if (fallbackLocales.Count == 0)
            {
                throw new Exception($"No supported language found for {targetLocale}");
            }

            var generators = new List<LanguageGenerator>();
            foreach (var locale in fallbackLocales)
            {
                if (this.TryGetGenerator(dialogContext, locale, out LanguageGenerator generator))
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
                    return await generator.Generate(dialogContext, template, data);
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
