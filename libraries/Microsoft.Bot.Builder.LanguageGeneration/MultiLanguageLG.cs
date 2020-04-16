using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// Multi locale Template Manager for language generation. This template manager will enumerate multi-locale LG files and will select
    /// the appropriate template using the current culture to perform template evaluation.
    /// </summary>
    public class MultiLanguageLG
    {
        private readonly LanguagePolicy languageFallbackPolicy;

        private readonly Dictionary<string, Templates> lgPerLocale;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageLG"/> class.
        /// </summary>
        /// <param name="localeLGFiles">A dictionary of locale and LG file.</param>
        /// <param name="defaultLanguage">Default language.</param>
        public MultiLanguageLG(Dictionary<string, string> localeLGFiles, string defaultLanguage = "")
        {
            lgPerLocale = new Dictionary<string, Templates>(StringComparer.OrdinalIgnoreCase);
            languageFallbackPolicy = new LanguagePolicy(defaultLanguage ?? string.Empty);

            if (localeLGFiles == null)
            {
                throw new ArgumentNullException(nameof(localeLGFiles));
            }

            foreach (var filesPerLocale in localeLGFiles)
            {
                lgPerLocale[filesPerLocale.Key] = Templates.ParseFile(filesPerLocale.Value);
            }
        }

        public MultiLanguageLG(Dictionary<string, Templates> templatesDict, string defaultLanguage = "")
        {
            lgPerLocale = new Dictionary<string, Templates>(StringComparer.OrdinalIgnoreCase);
            foreach (var templatesPair in templatesDict)
            {
                lgPerLocale.Add(templatesPair.Key, templatesPair.Value);
            }

            languageFallbackPolicy = new LanguagePolicy(defaultLanguage ?? string.Empty);
        }

        public object Generate(string template, object data, string locale)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            locale = locale ?? string.Empty;

            if (lgPerLocale.ContainsKey(locale))
            {
                return lgPerLocale[locale].EvaluateText(template, data);
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
                    if (lgPerLocale.ContainsKey(fallBackLocale))
                    {
                        return lgPerLocale[fallBackLocale].Evaluate(template, data);
                    }
                }
            }

            throw new Exception($"No LG responses found for locale: {locale}");
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
            //   "en-us" -> "en" -> "" 
            //   "en" -> ""
            // So that when we get a locale such as en-gb, we can try to resolve to "en-gb" then "en" then ""
            // See commented section for full sample of output of this function
            private static IDictionary<string, string[]> DefaultPolicy(string[] defaultLanguages = null)
            {
                if (defaultLanguages == null)
                {
                    defaultLanguages = new string[] { string.Empty };
                }

                var cultureCodes = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.IetfLanguageTag.ToLower()).ToList();
                var policy = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                foreach (var language in cultureCodes.Distinct())
                {
                    var lang = language.ToLower();
                    var fallback = new List<string>();
                    while (!string.IsNullOrEmpty(lang))
                    {
                        fallback.Add(lang);

                        var i = lang.LastIndexOf("-");
                        if (i > 0)
                        {
                            lang = lang.Substring(0, i);
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (language == string.Empty)
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
