using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Multi locale Template Manager for language generation. This template manager will enumerate multi-locale LG files and will select
    /// the appropriate template using the current culture to perform template evaluation.
    /// </summary>
    public class MultiLanguageLG
    {
        private readonly LanguagePolicy _languageFallbackPolicy;

        private readonly Dictionary<string, Templates> _lgPerLocale;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageLG"/> class.
        /// </summary>
        /// <param name="filePerLocale">Dictionary of locale and LG file.</param>
        /// <param name="defaultLanguage">Default language.</param>
        public MultiLanguageLG(Dictionary<string, string> filePerLocale, string defaultLanguage = "")
        {
            _lgPerLocale = new Dictionary<string, Templates>(StringComparer.OrdinalIgnoreCase);
            _languageFallbackPolicy = new LanguagePolicy(defaultLanguage);

            if (filePerLocale == null)
            {
                throw new ArgumentNullException(nameof(filePerLocale));
            }

            foreach (var item in filePerLocale)
            {
                _lgPerLocale[item.Key] = Templates.ParseFile(item.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageLG"/> class.
        /// </summary>
        /// <param name="templatesPerLocale">Dictionary of LG file templates per locale.</param>
        /// <param name="defaultLanguage">Default language.</param>
        public MultiLanguageLG(Dictionary<string, Templates> templatesPerLocale, string defaultLanguage = "")
        {
            _lgPerLocale = new Dictionary<string, Templates>(StringComparer.OrdinalIgnoreCase);
            foreach (var templatesPair in templatesPerLocale)
            {
                _lgPerLocale.Add(templatesPair.Key, templatesPair.Value);
            }

            _languageFallbackPolicy = new LanguagePolicy(defaultLanguage);
        }

        /// <summary>
        /// Generate template evaluate result.
        /// </summary>
        /// <param name="template">Template name.</param>
        /// <param name="data">Scope data.</param>
        /// <param name="locale">Locale info.</param>
        /// <returns>Evaluate result.</returns>
        public object Generate(string template, object data = null, string locale = "")
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            locale = locale ?? string.Empty;

            if (_lgPerLocale.ContainsKey(locale))
            {
                return _lgPerLocale[locale].Evaluate(template, data);
            }

            var fallbackLocales = new List<string>();
            if (_languageFallbackPolicy.ContainsKey(locale))
            {
                fallbackLocales.AddRange(_languageFallbackPolicy[locale]);
            }

            // append empty as fallback to end
            if (!string.IsNullOrEmpty(locale) && _languageFallbackPolicy.ContainsKey(string.Empty))
            {
                fallbackLocales.AddRange(_languageFallbackPolicy[string.Empty]);
            }

            if (fallbackLocales.Count == 0)
            {
                throw new ArgumentException($"No supported language found for {locale}");
            }

            foreach (var fallbackLocale in fallbackLocales)
            {
                if (_lgPerLocale.ContainsKey(fallbackLocale))
                {
                    return _lgPerLocale[fallbackLocale].Evaluate(template, data);
                }
            }

            throw new ArgumentException($"No LG responses found for locale: {locale}");
        }

        /// <summary>
        /// Language policy with fallback for each language as most specific to default en-us -> en -> default.
        /// </summary>
        private class LanguagePolicy : Dictionary<string, string[]>
        {
            // Keep this method for JSON deserialization 
            public LanguagePolicy()
                : base(DefaultPolicy(), StringComparer.OrdinalIgnoreCase)
            {
            }

            public LanguagePolicy(params string[] defaultLanguage)
                : base(DefaultPolicy(defaultLanguage), StringComparer.OrdinalIgnoreCase)
            {
            }

            // walk through all of the cultures and create a dictionary map with most specific to least specific
            // Example output "en-us" will generate fallback rule like this:
            //   "en-us" -> "en"
            //   "" -> defaultLanguages
            // So that when we get a locale such as en-gb, we can try to resolve to "en-gb" then "en"
            // See commented section for full sample of output of this function
            private static IDictionary<string, string[]> DefaultPolicy(string[] defaultLanguages = null)
            {
                if (defaultLanguages == null)
                {
                    defaultLanguages = new string[] { string.Empty };
                }

                var cultureCodes = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.IetfLanguageTag.ToLowerInvariant()).ToList();
                var policy = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                foreach (var language in cultureCodes.Distinct())
                {
                    var lang = language.ToLowerInvariant();
                    var fallback = new List<string>();
                    while (!string.IsNullOrEmpty(lang))
                    {
                        fallback.Add(lang);

                        var i = lang.LastIndexOf("-", StringComparison.Ordinal);
                        if (i > 0)
                        {
                            lang = lang.Substring(0, i);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(language))
                    {
                        // here we set the default
                        fallback.AddRange(defaultLanguages);
                    }

                    policy.Add(language, fallback.ToArray());
                }

                return policy;
            }
        }
    }
}
