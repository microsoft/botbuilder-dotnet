using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Generators
{
    public class MultiLanguageResourceLoader
    {
        public static Dictionary<string, List<IResource>> LoadResources(ResourceExplorer resourceExplorer)
        {
            var resourceMapping = new Dictionary<string, List<IResource>>();
            var allResources = resourceExplorer.GetResources("lg").ToList();
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
                        var resourcesWithSuchSuffix = allResources.Where(u => ParseLGFile(u.Id).language == suffix).ToList();
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
                            var resourcesWithEmptySuffix = allResources.Where(u => ParseLGFile(u.Id).language == string.Empty).ToList();
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

            return resourceMapping;
        }

        public static (string prefix, string language) ParseLGFile(string lgFile)
        {
            if (string.IsNullOrEmpty(lgFile) || !lgFile.EndsWith(".lg"))
            {
                return (lgFile, string.Empty);
            }

            var fileName = lgFile.Substring(0, lgFile.Length - ".lg".Length);

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
    }
}
