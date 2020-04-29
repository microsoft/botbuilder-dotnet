// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    /// <summary>
    /// load all lg resource and split them into different language group.
    /// </summary>
    public class LGResourceLoader
    {
        public static Dictionary<string, IList<Resource>> GroupByLocale(ResourceExplorer resourceExplorer)
        {
            var resourceMapping = new Dictionary<string, IList<Resource>>();
            var allResources = resourceExplorer.GetResources("lg");
            var languagePolicy = new LanguagePolicy();
            foreach (var item in languagePolicy)
            {
                var locale = item.Key;
                var suffixs = item.Value;
                var existNames = new HashSet<string>();
                foreach (var suffix in suffixs)
                {
                    if (string.IsNullOrEmpty(locale) || !string.IsNullOrEmpty(suffix))
                    {
                        var resourcesWithSuchSuffix = allResources.Where(u => ParseLGFileName(u.Id).language == suffix);
                        foreach (var resourceWithSuchSuffix in resourcesWithSuchSuffix)
                        {
                            var resourceName = resourceWithSuchSuffix.Id;
                            var length = string.IsNullOrEmpty(suffix) ? 3 : 4;
                            var prefixName = resourceName.Substring(0, resourceName.Length - suffix.Length - length);
                            if (!existNames.Contains(prefixName))
                            {
                                existNames.Add(prefixName);
                                if (!resourceMapping.ContainsKey(locale))
                                {
                                    resourceMapping.Add(locale, new List<Resource> { resourceWithSuchSuffix });
                                }
                                else
                                {
                                    resourceMapping[locale].Add(resourceWithSuchSuffix);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (resourceMapping.ContainsKey(locale))
                        {
                            var resourcesWithEmptySuffix = allResources.Where(u => ParseLGFileName(u.Id).language == string.Empty);
                            foreach (var resourceWithEmptySuffix in resourcesWithEmptySuffix)
                            {
                                var resourceName = resourceWithEmptySuffix.Id;
                                var prefixName = resourceName.Substring(0, resourceName.Length - 3);
                                if (!existNames.Contains(prefixName))
                                {
                                    existNames.Add(prefixName);
                                    resourceMapping[locale].Add(resourceWithEmptySuffix);
                                }
                            }
                        }
                    }
                }
            }

            return FallbackMultiLangResource(resourceMapping);
        }

        /// <summary>
        /// parse lg file name into prefix and language.
        /// </summary>
        /// <param name="lgFileName">lg input name.</param>
        /// <returns>get the name and language.</returns>
        public static (string prefix, string language) ParseLGFileName(string lgFileName)
        {
            if (string.IsNullOrEmpty(lgFileName) || !lgFileName.EndsWith(".lg"))
            {
                return (lgFileName, string.Empty);
            }

            var fileName = lgFileName.Substring(0, lgFileName.Length - ".lg".Length);

            var lastDot = fileName.LastIndexOf(".");
            if (lastDot > 0)
            {
                return (fileName.Substring(0, lastDot), fileName.Substring(lastDot + 1));
            }
            else
            {
                return (fileName, string.Empty);
            }
        }

        /// <summary>
        /// Get the fall back locale from the optional locales. for example
        /// en-us, is a locale from English. But the option locales has [en, ''],
        /// So,en would be picked.
        /// </summary>
        /// <param name="locale">current locale.</param>
        /// <param name="optionalLocales">option locales.</param>
        /// <returns>the final locale.</returns>
        public static string FallbackLocale(string locale, IList<string> optionalLocales)
        {
            if (optionalLocales == null)
            {
                throw new ArgumentNullException();
            }

            if (optionalLocales.Contains(locale))
            {
                return locale;
            }

            var languagePolicy = new LanguagePolicy();

            if (languagePolicy.ContainsKey(locale))
            {
                var fallbackLocals = languagePolicy[locale];
                foreach (var fallbackLocal in fallbackLocals)
                {
                    if (optionalLocales.Contains(fallbackLocal))
                    {
                        return fallbackLocal;
                    }
                }
            }
            else if (optionalLocales.Contains(string.Empty))
            {
                return string.Empty;
            }

            throw new Exception($"there is no locale fallback for {locale}");
        }

        /// <summary>
        /// fallback resource.
        /// for example, en-us -> [1.en.lg, 2.lg].   en -> [1.en.lg, 2.lg]
        /// result will be :en -> [1.en.lg, 2.lg]. and use fallback to find the resources.
        /// </summary>
        /// <param name="resourceMapping">input resource mapping.</param>
        /// <returns>merged resource mapping.</returns>
        private static Dictionary<string, IList<Resource>> FallbackMultiLangResource(Dictionary<string, IList<Resource>> resourceMapping)
        {
            var resourcePoolDict = new Dictionary<string, IList<Resource>>();
            foreach (var languageItem in resourceMapping)
            {
                var currentLocale = languageItem.Key;
                var currentResourcePool = languageItem.Value;
                var sameResourcePool = resourcePoolDict.FirstOrDefault(u => HasSameResourcePool(u.Value, languageItem.Value));
                var existLocale = sameResourcePool.Key;

                if (existLocale == null)
                {
                    resourcePoolDict.Add(currentLocale, currentResourcePool);
                }
                else
                {
                    var newLocale = FindCommonAncestorLocale(existLocale, currentLocale);
                    if (!string.IsNullOrWhiteSpace(newLocale) && newLocale != existLocale)
                    {
                        resourcePoolDict.Remove(existLocale);
                        resourcePoolDict.Add(newLocale, currentResourcePool);
                    }
                }
            }

            return resourcePoolDict;
        }

        /// <summary>
        /// find the common parent locale, for example
        /// en-us, en-gb, has the same parent locale: en.
        /// and en-us, fr, has the no same parent locale.
        /// </summary>
        /// <param name="locale1">first locale.</param>
        /// <param name="locale2">second locale.</param>
        /// <returns>the most closest common ancestor local.</returns>
        private static string FindCommonAncestorLocale(string locale1, string locale2)
        {
            var policy = new LanguagePolicy();
            if (!policy.ContainsKey(locale1) || !policy.ContainsKey(locale2))
            {
                return string.Empty;
            }

            var key1Policy = policy[locale1];
            var key2Policy = policy[locale2];
            foreach (var key1Language in key1Policy)
            {
                foreach (var key2Language in key2Policy)
                {
                    if (key1Language == key2Language)
                    {
                        return key1Language;
                    }
                }
            }

            return string.Empty;
        }

        private static bool HasSameResourcePool(IList<Resource> resourceMapping1, IList<Resource> resourceMapping2)
        {
            if (resourceMapping1 == null && resourceMapping2 == null)
            {
                return true;
            }

            if ((resourceMapping1 == null && resourceMapping2 != null)
                || (resourceMapping1 != null && resourceMapping2 == null)
                || resourceMapping1.Count != resourceMapping2.Count)
            {
                return false;
            }

            resourceMapping1 = resourceMapping1.OrderBy(u => u.Id).ToList();
            resourceMapping2 = resourceMapping2.OrderBy(u => u.Id).ToList();

            for (var i = 0; i < resourceMapping1.Count; i++)
            {
                if (resourceMapping1[i].Id != resourceMapping2[i].Id)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
