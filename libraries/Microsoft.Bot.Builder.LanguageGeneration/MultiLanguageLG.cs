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
        private readonly LanguagePolicy languagePolicy;

        private readonly Dictionary<string, Templates> lgPerLocale;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageLG"/> class.
        /// </summary>
        /// <param name="localeLGFiles">A dictionary of locale and LG file.</param>
        /// <param name="defaultLanguages">Default language.</param>
        public MultiLanguageLG(Dictionary<string, string> localeLGFiles, params string[] defaultLanguages)
        {
            lgPerLocale = new Dictionary<string, Templates>(StringComparer.OrdinalIgnoreCase);
            languagePolicy = new LanguagePolicy(defaultLanguages);

            if (localeLGFiles == null)
            {
                throw new ArgumentNullException(nameof(localeLGFiles));
            }

            foreach (var filesPerLocale in localeLGFiles)
            {
                lgPerLocale[filesPerLocale.Key] = Templates.ParseFile(filesPerLocale.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiLanguageLG"/> class.
        /// </summary>
        /// <param name="templatesDict">A dictionary of LG file templates.</param>
        /// <param name="defaultLanguages">Default language.</param>
        public MultiLanguageLG(Dictionary<string, Templates> templatesDict, params string[] defaultLanguages)
        {
            lgPerLocale = new Dictionary<string, Templates>(StringComparer.OrdinalIgnoreCase);
            foreach (var templatesPair in templatesDict)
            {
                lgPerLocale.Add(templatesPair.Key, templatesPair.Value);
            }

            languagePolicy = new LanguagePolicy(defaultLanguages);
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
                return lgPerLocale[locale].Evaluate(template, data);
            }

            var fallbackLocales = new List<string>();
            if (languagePolicy.ContainsKey(locale))
            {
                fallbackLocales.AddRange(languagePolicy[locale]);
            }

            // append empty as fallback to end
            if (locale != string.Empty && languagePolicy.ContainsKey(string.Empty))
            {
                fallbackLocales.AddRange(languagePolicy[string.Empty]);
            }

            if (fallbackLocales.Count == 0)
            {
                throw new Exception($"No supported language found for {locale}");
            }

            foreach (var fallbackLocale in fallbackLocales)
            {
                if (lgPerLocale.ContainsKey(fallbackLocale))
                {
                    return lgPerLocale[fallbackLocale].Evaluate(template, data);
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
