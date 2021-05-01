using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions;
using AdaptiveExpressions.Memory;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.LanguageGeneration;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive
{
    /// <summary>
    /// Register lg templates into Expression.
    /// Using prefix lg. to avoid the conflict with pre-built functions.
    /// </summary>
    public static class TemplatesRegisterExtensions
    {
        /// <summary>
        /// Register Template functions.
        /// </summary>
        /// <param name="dialogContext">Dialog context.</param>
        /// <param name="generator">Language generator.</param>
        public static void RegisterTemplateFunctions(this DialogContext dialogContext, LanguageGenerator generator)
        {
            var lgm = dialogContext.Services.Get<LanguageGeneratorManager>();
            var templateNames = GetTemplates(generator, dialogContext).Distinct();

            var prebuildFunctionNames = Expression.Functions.Values.Select(u => u.Type);
            foreach (var templateName in templateNames)
            {
                var functionType = prebuildFunctionNames.Contains(templateName) ? "lg." + templateName : templateName;
                Expression.Functions.Add(functionType, new ExpressionEvaluator(
                    functionType,
                    (expression, state, options) =>
                    {
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
                                        var (result, error) = EvaluateTemplate(templateGenerator.LG, templateName, expression, state, options, currentLocale);
                                        if (error == null)
                                        {
                                            return (result, null);
                                        }
                                    }

                                    // Alias inject
                                    var pointIndex = templateName.IndexOf('.');
                                    if (pointIndex > 0)
                                    {
                                        var alias = templateName.Substring(0, pointIndex);
                                        if (templateGenerator.LG.NamedReferences.ContainsKey(alias))
                                        {
                                            var realTemplateName = templateName.Substring(pointIndex + 1);
                                            var (result, error) = EvaluateTemplate(templateGenerator.LG.NamedReferences[alias], realTemplateName, expression, state, options, currentLocale);
                                            if (error == null)
                                            {
                                                return (result, null);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        throw new Exception($"{templateName} does not have an evaluator");
                    }));
            }
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

        private static List<string> GetTemplates(LanguageGenerator languageGenerator, DialogContext dialogContext)
        {
            var templateNames = new List<string>();
            if (languageGenerator is TemplateEngineLanguageGenerator templateGenerator)
            {
                templateNames.AddRange(templateGenerator.LG.AllTemplates.Select(u => u.Name));
                var namedReferences = templateGenerator.LG.NamedReferences;
                foreach (var nemdReference in namedReferences)
                {
                    templateNames.AddRange(nemdReference.Value.AllTemplates.Select(u => nemdReference.Key + "." + u.Name));
                }

                return templateNames;
            }

            if (languageGenerator is ResourceMultiLanguageGenerator resourceGenerator)
            {
                var lgm = dialogContext.Services.Get<LanguageGeneratorManager>();
                if (lgm != null)
                {
                    var generators = lgm.LanguageGenerators;
                    foreach (var generator in generators)
                    {
                        var (targetResourceName, _) = LGResourceLoader.ParseLGFileName(generator.Key);
                        var (currentResourceName, _) = LGResourceLoader.ParseLGFileName(resourceGenerator.ResourceId);
                        if (targetResourceName == currentResourceName)
                        {
                            templateNames.AddRange(GetTemplates(generator.Value, dialogContext));
                        }
                    }
                }
            }

            return templateNames;
        }

        private static (object result, string error) EvaluateTemplate(LanguageGeneration.Templates templates, string templateName, Expression expression, IMemory state, Options options, string currentLocale)
        {
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                var parameters = templates.AllTemplates.ToList().First(u => u.Name == templateName).Parameters;
                var newScope = parameters.Zip(args, (k, v) => new { k, v })
                    .ToDictionary(x => x.k, x => x.v);
                var scope = new StackedMemory();
                scope.Push(state);
                scope.Push(new SimpleObjectMemory(newScope));
                var lgOpt = new EvaluationOptions() { Locale = currentLocale, NullSubstitution = options.NullSubstitution };
                var result = templates.Evaluate(templateName, scope, lgOpt);
                return (result, error);
            }

            return (null, error);
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
    }
}
