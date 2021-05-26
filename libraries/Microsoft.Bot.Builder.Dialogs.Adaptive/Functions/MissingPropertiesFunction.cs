// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Functions
{
    /// <summary>
    /// Defines missingProperties(templateName) expression function.
    /// </summary>
    /// <remarks>
    /// This expression will get all variables the template contains.
    /// </remarks>
    public class MissingPropertiesFunction : ExpressionEvaluator
    {
        /// <summary>
        /// Function identifier name.
        /// </summary>
        public const string Name = "missingProperties";

        private static DialogContext dialogContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingPropertiesFunction"/> class.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        public MissingPropertiesFunction(DialogContext context)
            : base(Name, Function, ReturnType.Array, FunctionUtils.ValidateUnaryString)
        {
            dialogContext = context;
        }

        private static (object value, string error) Function(Expression expression, IMemory state, Options options)
        {
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error != null)
            {
                return (null, error);
            }

            var lgm = dialogContext.Services.Get<LanguageGeneratorManager>();
            var templateName = args[0]?.ToString();
            var currentLocale = GetCurrentLocale(state, options);

            if (state.TryGetValue("dialogclass.generator.resourceId", out var resourceId))
            {
                var (resourceName, locale) = LGResourceLoader.ParseLGFileName(resourceId.ToString());

                var languagePolicy = GetLanguagePolicy(state);

                var fallbackLocales = GetFallbackLocales(languagePolicy, currentLocale);

                var generators = new List<LanguageGenerator>();
                generators.AddRange(GetGenerators(lgm.LanguageGenerators, fallbackLocales, resourceName + ".lg"));

                if (!string.IsNullOrEmpty(locale))
                {
                    generators.AddRange(GetGenerators(lgm.LanguageGenerators, fallbackLocales, resourceId.ToString()));
                }

                foreach (var generator in generators)
                {
                    if (generator is TemplateEngineLanguageGenerator templateGenerator)
                    {
                        if (templateGenerator.LG.AllTemplates.Any(u => u.Name == templateName))
                        {
                            var analyzerResults = templateGenerator.LG.AnalyzeTemplate(templateName);
                            return (analyzerResults.Variables, null);
                        }

                        // Alias inject
                        var pointIndex = templateName.IndexOf('.');
                        if (pointIndex > 0)
                        {
                            var alias = templateName.Substring(0, pointIndex);
                            if (templateGenerator.LG.NamedReferences.ContainsKey(alias))
                            {
                                var realTemplateName = templateName.Substring(pointIndex + 1);
                                var analyzerResults = templateGenerator.LG.NamedReferences[alias].AnalyzeTemplate(realTemplateName);
                                return (analyzerResults.Variables, null);
                            }
                        }
                    }
                }
            }

            return (new List<string>(), null);
        }

        private static string GetCurrentLocale(IMemory memory, Options options)
        {
            string currentLocale;
            if (memory.TryGetValue("turn.locale", out var locale))
            {
                currentLocale = locale.ToString();
            }
            else
            {
                currentLocale = options.Locale;
            }

            return currentLocale;
        }

        private static LanguagePolicy GetLanguagePolicy(IMemory memory)
        {
            // order: dialogclass.generator.languagePoilcy ?? turn.languagePolicy ?? default policy

            object languagePolicyObj;
            var getLanguagePolicy = false;
            if (!memory.TryGetValue("dialogclass.generator.languagePolicy", out languagePolicyObj))
            {
                if (memory.TryGetValue("turn.languagePolicy", out languagePolicyObj))
                {
                    getLanguagePolicy = true;
                }
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

        private static List<string> GetFallbackLocales(LanguagePolicy languagePolicy, string currentLocale)
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

        private static List<LanguageGenerator> GetGenerators(ConcurrentDictionary<string, LanguageGenerator> generators, List<string> fallbackLocales, string resourceId)
        {
            var result = new List<LanguageGenerator>();
            foreach (var fallbackLocale in fallbackLocales)
            {
                var id = string.IsNullOrEmpty(fallbackLocale) ? resourceId : resourceId.Replace(".lg", $".{fallbackLocale}.lg");
                if (generators.ContainsKey(id))
                {
                    result.Add(generators[id]);
                }
            }

            return result;
        }
    }
}
