// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var targetLocale = dialogContext.GetLocale();
            var fallbackLocales = GetFallbackLocales(languagePolicy, targetLocale);
            var generators = GetGenerators(dialogContext, fallbackLocales);

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

        /// <summary>
        /// Method to get missing properties.
        /// </summary>
        /// <param name="dialogContext">dialogContext.</param>
        /// <param name="templateBody">template or [templateId].</param>
        /// <param name="state">Memory state.</param>
        /// <param name="options">Options.</param>
        /// <param name="cancellationToken">the <see cref="CancellationToken"/> for the task.</param>
        /// <returns>Property list.</returns>
        public override List<string> MissingProperties(DialogContext dialogContext, string templateBody, IMemory state = null, Options options = null, CancellationToken cancellationToken = default)
        {
            var currentLocale = GetCurrentLocale(state, options);
            var languagePolicy = GetLanguagePolicy(state);
            var fallbackLocales = GetFallbackLocales(languagePolicy, currentLocale);
            var generators = GetGenerators(dialogContext, fallbackLocales);

            if (generators.Count == 0)
            {
                generators.Add(new TemplateEngineLanguageGenerator());
            }

            foreach (var generator in generators)
            {
                try
                {
                    return generator.MissingProperties(dialogContext, templateBody, state, options, cancellationToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types (catch any exception and add it to the errors list).
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // ignore
                }
            }

            return new List<string>();
        }

        private string GetCurrentLocale(IMemory memory, Options options)
        {
            string currentLocale;
            if (memory.TryGetValue(TurnPath.Locale, out var locale))
            {
                currentLocale = locale.ToString();
            }
            else
            {
                currentLocale = options.Locale;
            }

            return currentLocale;
        }

        private LanguagePolicy GetLanguagePolicy(IMemory memory)
        {
            // order: dialogclass.generator.languagePoilcy ?? turn.languagePolicy ?? default policy

            if (LanguagePolicy != null)
            {
                return LanguagePolicy;
            }

            object languagePolicyObj;
            var getLanguagePolicy = false;
            if (memory.TryGetValue(TurnPath.LanguagePolicy, out languagePolicyObj))
            {
                getLanguagePolicy = true;
            }

            LanguagePolicy policy;
            if (!getLanguagePolicy)
            {
                policy = new LanguagePolicy();
            }
            else
            {
                policy = JObject.FromObject(languagePolicyObj).ToObject<LanguagePolicy>();
            }

            return policy;
        }

        private List<string> GetFallbackLocales(LanguagePolicy languagePolicy, string currentLocale)
        {
            var fallbackLocales = new List<string>();

            if (languagePolicy.ContainsKey(currentLocale))
            {
                fallbackLocales.AddRange(languagePolicy[currentLocale]);
            }

            // append empty as fallback to end
            if (currentLocale.Length != 0 && languagePolicy.ContainsKey(string.Empty))
            {
                fallbackLocales.AddRange(languagePolicy[string.Empty]);
            }

            if (fallbackLocales.Count == 0)
            {
                throw new InvalidOperationException($"No supported language found for {currentLocale}");
            }

            return fallbackLocales;
        }

        private List<LanguageGenerator> GetGenerators(DialogContext dc, List<string> fallbackLocales)
        {
            var generators = new List<LanguageGenerator>();
            foreach (var locale in fallbackLocales)
            {
                if (TryGetGenerator(dc, locale, out var generator))
                {
                    generators.Add(generator);
                }
            }

            return generators;
        }
    }
}
