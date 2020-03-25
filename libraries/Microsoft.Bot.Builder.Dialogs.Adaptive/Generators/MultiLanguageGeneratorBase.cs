// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public LanguagePolicy LanguagePolicy { get; set; } = new LanguagePolicy();

        /// <summary>
        /// Abstract method to get an ILanguageGenerator by locale.
        /// </summary>
        /// <param name="context">context.</param>
        /// <param name="locale">locale.</param>
        /// <param name="generator">generator to return.</param>
        /// <returns>true if found.</returns>
        public abstract bool TryGetGenerator(ITurnContext context, string locale, out LanguageGenerator generator);

        /// <summary>
        /// Find a language generator that matches the current context locale.
        /// </summary>
        /// <param name="turnContext">Context for the current turn of conversation.</param>
        /// <param name="template">The template.</param>
        /// <param name="data">data to bind to.</param>
        /// <returns>The generator.</returns>
        public override async Task<string> Generate(ITurnContext turnContext, string template, object data)
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

            var generators = new List<LanguageGenerator>();
            foreach (var locale in locales)
            {
                if (this.TryGetGenerator(turnContext, locale, out LanguageGenerator generator))
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
