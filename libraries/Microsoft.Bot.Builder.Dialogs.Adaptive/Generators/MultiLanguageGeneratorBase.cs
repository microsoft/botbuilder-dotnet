// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        /// <summary>
        /// Gets or sets the language policy.
        /// </summary>
        /// <value>
        /// Language policy.
        /// </value>
        [JsonProperty("languagePolicy")]
#pragma warning disable CA2227 // Collection properties should be read only (we can't change this without breaking binary compat)
        public LanguagePolicy LanguagePolicy { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

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
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>The generator.</returns>
        public override async Task<object> GenerateAsync(DialogContext dialogContext, string template, object data, CancellationToken cancellationToken = default)
        {
            // priority 
            // 1. local policy
            // 2. shared policy in turnContext
            // 3. default policy
            var languagePolicy = this.LanguagePolicy ?? 
                                dialogContext.Services.Get<LanguagePolicy>() ?? 
                                new LanguagePolicy();

            // see if we have any locales that match
            var fallbackLocales = new List<string>();
            var targetLocale = dialogContext.GetLocale();

            if (languagePolicy.ContainsKey(targetLocale))
            {
                fallbackLocales.AddRange(languagePolicy[targetLocale]);
            }
            
            // append empty as fallback to end
            if (targetLocale.Length != 0 && languagePolicy.ContainsKey(string.Empty))
            {
                fallbackLocales.AddRange(languagePolicy[string.Empty]);
            }

            if (fallbackLocales.Count == 0)
            {
                throw new InvalidOperationException($"No supported language found for {targetLocale}");
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
                throw new InvalidOperationException($"No generator found for language {targetLocale}");
            }

            var errors = new List<string>();
            foreach (var generator in generators)
            {
                try
                {
                    return await generator.GenerateAsync(dialogContext, template, data, cancellationToken).ConfigureAwait(false);
                }
#pragma warning disable CA1031 // Do not catch general exception types (catch any exception and add it to the errors list).
                catch (Exception err)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    errors.Add(err.Message);
                }
            }

            throw new InvalidOperationException(string.Join(",\n", errors.Distinct()));
        }
    }
}
