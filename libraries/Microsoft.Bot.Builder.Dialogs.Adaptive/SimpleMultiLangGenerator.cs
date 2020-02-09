// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Expressions.Properties;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Multi locale Template Manager for language generation. This template manager will enumerate multi-locale LG files and will select
    /// the appropriate template using the current culture to perform template evaluation.
    /// </summary>
    public class SimpleMultiLangGenerator : ILanguageGenerator
    {
        private readonly LanguagePolicy languageFallbackPolicy;

        private readonly Dictionary<string, LGFile> lgFilesPerLocale;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiLangGenerator"/> class.
        /// </summary>
        /// <param name="localeLGFiles">A dictionary of locale and LG file.</param>
        public SimpleMultiLangGenerator(Dictionary<string, string> localeLGFiles)
        {
            lgFilesPerLocale = new Dictionary<string, LGFile>(StringComparer.OrdinalIgnoreCase);
            languageFallbackPolicy = new LanguagePolicy();

            if (localeLGFiles == null)
            {
                throw new ArgumentNullException(nameof(localeLGFiles));
            }

            foreach (var filesPerLocale in localeLGFiles)
            {
                lgFilesPerLocale[filesPerLocale.Key] = LGParser.ParseFile(filesPerLocale.Value);
            }
        }

        public async Task<string> Generate(ITurnContext turnContext, string template, object data)
        {
            await Task.CompletedTask;

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var locale = turnContext.Activity.Locale ?? string.Empty;

            if (lgFilesPerLocale.ContainsKey(locale))
            {
                return lgFilesPerLocale[locale].Evaluate(template, data).ToString();
            }
            else
            {
                var locales = new string[] { string.Empty };
                if (!languageFallbackPolicy.TryGetValue(locale, out locales))
                {
                    if (!languageFallbackPolicy.TryGetValue(string.Empty, out locales))
                    {
                        throw new Exception($"No supported language found for {locale}");
                    }
                }

                foreach (var fallBackLocale in locales)
                {
                    if (lgFilesPerLocale.ContainsKey(fallBackLocale))
                    {
                        return lgFilesPerLocale[fallBackLocale].Evaluate(template, data).ToString();
                    }
                }
            }

            throw new Exception($"No LG responses found for locale: {locale}");
        }
    }
}
