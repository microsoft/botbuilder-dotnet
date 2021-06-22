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
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Functions
{
    /// <summary>
    /// Defines missingProperties(template) expression function.
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

        private const string LanguagePolicyInDialogPath = "dialogclass.generator.languagePolicy";

        private const string ResourceIdInDialogPath = "dialogclass.generator.resourceId";

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
            var templateBody = args[0]?.ToString();
            var currentLocale = GetCurrentLocale(state, options);

            if (state.TryGetValue(ResourceIdInDialogPath, out var resourceId))
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
                        var tempTemplateName = $"{Templates.InlineTemplateIdPrefix}{Guid.NewGuid():N}";

                        var multiLineMark = "```";

                        templateBody = !templateBody.Trim().StartsWith(multiLineMark, StringComparison.Ordinal) && templateBody.Contains('\n')
                               ? $"{multiLineMark}{templateBody}{multiLineMark}" : templateBody;

                        templateGenerator.LG.AddTemplate(tempTemplateName, null, $"- {templateBody}");
                        var analyzerResults = templateGenerator.LG.AnalyzeTemplate(tempTemplateName);

                        // Delete it after the analyzer
                        templateGenerator.LG.DeleteTemplate(tempTemplateName);
                        return (analyzerResults.Variables, null);
                    }
                }
            }

            return (new List<string>(), null);
        }

        private static string GetCurrentLocale(IMemory memory, Options options)
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

        private static LanguagePolicy GetLanguagePolicy(IMemory memory)
        {
            // order: dialogclass.generator.languagePoilcy ?? turn.languagePolicy ?? default policy

            object languagePolicyObj;
            var getLanguagePolicy = false;
            if (!memory.TryGetValue(LanguagePolicyInDialogPath, out languagePolicyObj))
            {
                if (memory.TryGetValue(TurnPath.LanguagePolicy, out languagePolicyObj))
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
