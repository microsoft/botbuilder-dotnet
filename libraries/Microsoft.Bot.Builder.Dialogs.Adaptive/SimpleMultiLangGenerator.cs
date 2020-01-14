// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Multi locale Template Manager for language generation. This template manager will enumerate multi-locale LG files and will select
    /// the appropriate template using the current culture to perform template evaluation.
    /// </summary>
    public class SimpleMultiLangGenerator : ILanguageGenerator
    {
        private readonly LanguagePolicy languageFallbackPolicy;

        private readonly string localeDefault;

        private readonly Dictionary<string, LGFile> lgFilesPerLocale;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleMultiLangGenerator"/> class.
        /// </summary>
        /// <param name="localeLGFiles">A dictionary of locale and LG file(s).</param>
        /// <param name="fallbackLocale">The default fallback locale to use.</param>
        public SimpleMultiLangGenerator(Dictionary<string, string> localeLGFiles, string fallbackLocale)
        {
            lgFilesPerLocale = new Dictionary<string, LGFile>(StringComparer.OrdinalIgnoreCase);
            languageFallbackPolicy = new LanguagePolicy();
            localeDefault = fallbackLocale;

            if (localeLGFiles == null)
            {
                throw new ArgumentNullException(nameof(localeLGFiles));
            }

            if (string.IsNullOrEmpty(fallbackLocale))
            {
                throw new ArgumentNullException(nameof(fallbackLocale));
            }

            foreach (var filesPerLocale in localeLGFiles)
            {
                lgFilesPerLocale[filesPerLocale.Key] = LGParser.ParseFile(filesPerLocale.Value);
            }
        }

        public async Task<Activity> Generate(ITurnContext turnContext, string template, object data)
        {
            if (templateName == null)
            {
                throw new ArgumentNullException(nameof(templateName));
            }

            // By default we use the locale for the current culture, if a locale is provided then we ignore this.
            var locale = localeOverride ?? CultureInfo.CurrentUICulture.Name;

            // Do we have a template engine for this locale?
            if (lgFilesPerLocale.ContainsKey(locale))
            {
                return ActivityFactory.CreateActivity(lgFilesPerLocale[locale].EvaluateTemplate(templateName, data).ToString());
            }
            else
            {
                // We don't have a set of matching responses for this locale so we apply fallback policy to find options.
                languageFallbackPolicy.TryGetValue(locale, out string[] locales);
                {
                    // If no fallback options were found then we fallback to the default and log.
                    if (!languageFallbackPolicy.TryGetValue(localeDefault, out locales))
                    {
                        throw new Exception($"No LG responses found for {locale} or when attempting to fallback to '{localeDefault}'");
                    }
                }

                // Work through the fallback hierarchy to find a response
                foreach (var fallBackLocale in locales)
                {
                    if (lgFilesPerLocale.ContainsKey(fallBackLocale))
                    {
                        return ActivityFactory.CreateActivity(lgFilesPerLocale[fallBackLocale].EvaluateTemplate(templateName, data).ToString());
                    }
                }
            }

            throw new Exception($"No LG responses found for {locale} or when attempting to fallback to '{localeDefault}'");
        }
    }
}
