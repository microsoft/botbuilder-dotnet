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
    public class MultiLanguageResourceLoader
    {
        public static Dictionary<string, IList<IResource>> Load(ResourceExplorer resourceExplorer)
        {
            var resourceMapping = new Dictionary<string, IList<IResource>>();
            var allResources = resourceExplorer.GetResources("lg");
            var languagePolicy = new LanguagePolicy();
            foreach (var item in languagePolicy)
            {
                var local = item.Key;
                var suffixs = item.Value;
                var existNames = new HashSet<string>();
                foreach (var suffix in suffixs)
                {
                    if (string.IsNullOrEmpty(local) || !string.IsNullOrEmpty(suffix))
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
                                if (!resourceMapping.ContainsKey(local))
                                {
                                    resourceMapping.Add(local, new List<IResource> { resourceWithSuchSuffix });
                                }
                                else
                                {
                                    resourceMapping[local].Add(resourceWithSuchSuffix);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (resourceMapping.ContainsKey(local))
                        {
                            var resourcesWithEmptySuffix = allResources.Where(u => ParseLGFileName(u.Id).language == string.Empty);
                            foreach (var resourceWithEmptySuffix in resourcesWithEmptySuffix)
                            {
                                var resourceName = resourceWithEmptySuffix.Id;
                                var prefixName = resourceName.Substring(0, resourceName.Length - 3);
                                if (!existNames.Contains(prefixName))
                                {
                                    existNames.Add(prefixName);
                                    resourceMapping[local].Add(resourceWithEmptySuffix);
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
        /// Get the fall back local from the optional locals. for example
        /// en-us, is a local from English. But the option locals has [en, ''],
        /// So,en would be picked.
        /// </summary>
        /// <param name="local">current local.</param>
        /// <param name="optionalLocals">option locals.</param>
        /// <returns>the final local.</returns>
        public static string FallbackLocal(string local, IList<string> optionalLocals)
        {
            if (optionalLocals == null)
            {
                throw new ArgumentNullException();
            }

            if (optionalLocals.Contains(local))
            {
                return local;
            }

            var languagePolicy = new LanguagePolicy();

            if (languagePolicy.ContainsKey(local))
            {
                var fallbackLocals = languagePolicy[local];
                foreach (var fallbackLocal in fallbackLocals)
                {
                    if (optionalLocals.Contains(fallbackLocal))
                    {
                        return fallbackLocal;
                    }
                }
            }
            else if (optionalLocals.Contains(string.Empty))
            {
                return string.Empty;
            }

            throw new Exception($"there is no local fallback for {local}");
        }

        /// <summary>
        /// fallback resource.
        /// for example, en-us -> [1.en.lg, 2.lg].   en -> [1.en.lg, 2.lg]
        /// result will be :en -> [1.en.lg, 2.lg]. and use fallback to find the resources.
        /// </summary>
        /// <param name="resourceMapping">input resource mapping.</param>
        /// <returns>merged resource mapping.</returns>
        private static Dictionary<string, IList<IResource>> FallbackMultiLangResource(Dictionary<string, IList<IResource>> resourceMapping)
        {
            var resourcePoolDict = new Dictionary<string, IList<IResource>>();
            foreach (var languageItem in resourceMapping)
            {
                var currentLocal = languageItem.Key;
                var currentResourcePool = languageItem.Value;
                var sameResourcePool = resourcePoolDict.FirstOrDefault(u => HasSameResourcePool(u.Value, languageItem.Value));
                var existLocal = sameResourcePool.Key;

                if (existLocal == null)
                {
                    resourcePoolDict.Add(currentLocal, currentResourcePool);
                }
                else
                {
                    var newLocal = FindCommonAncestorLocal(existLocal, currentLocal);
                    if (!string.IsNullOrWhiteSpace(newLocal) && newLocal != existLocal)
                    {
                        resourcePoolDict.Remove(existLocal);
                        resourcePoolDict.Add(newLocal, currentResourcePool);
                    }
                }
            }

            return resourcePoolDict;
        }

        /// <summary>
        /// find the common parent local, for example
        /// en-us, en-gb, has the same parent local: en.
        /// and en-us, fr, has the no same parent local.
        /// </summary>
        /// <param name="local1">first local.</param>
        /// <param name="local2">second local.</param>
        /// <returns>the most closest common ancestor local.</returns>
        private static string FindCommonAncestorLocal(string local1, string local2)
        {
            var policy = new LanguagePolicy();
            if (!policy.ContainsKey(local1) || !policy.ContainsKey(local2))
            {
                return string.Empty;
            }

            var key1Policy = policy[local1];
            var key2Policy = policy[local2];
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

        private static bool HasSameResourcePool(IList<IResource> resourceMapping1, IList<IResource> resourceMapping2)
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
