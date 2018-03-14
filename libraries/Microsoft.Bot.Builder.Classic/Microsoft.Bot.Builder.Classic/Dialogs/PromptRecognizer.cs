using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Bot.Schema;
using System.Resources;
using System.Globalization;
using Microsoft.Bot.Builder.Classic.Dialogs.Internals;
using System.Collections.Concurrent;
using Microsoft.Bot.Builder.Classic.Resource;

namespace Microsoft.Bot.Builder.Classic.Dialogs
{
    public class RecognizeEntity<T>
    {
        public T Entity { get; set; }
        public double Score { get; set; }
    }
    public interface IPromptRecognizeNumbersOptions
    {
        /// <summary>
        /// (Optional) Minimum value allowed.
        /// </summary>
        double? MinValue { get; set; }

        /// <summary>
        /// (Optional) Maximum value allowed.
        /// </summary>
        double? MaxValue { get; set; }

        /// <summary>
        /// (Optional) If true, then only integers will be recognized.
        /// </summary>
        bool? IntegerOnly { get; set; }
    }

    public class PromptRecognizeNumbersOptions : IPromptRecognizeNumbersOptions
    {
        /// <summary>
        /// (Optional) Minimum value allowed.
        /// </summary>
        public double? MinValue { get; set; }
        /// <summary>
        /// (Optional) Maximum value allowed.
        /// </summary>
        public double? MaxValue { get; set; }
        /// <summary>
        /// (Optional) If true, then only integers will be recognized.
        /// </summary>
        public bool? IntegerOnly { get; set; }
    }

    public interface IPromptRecognizeValuesOptions
    {
        bool? AllowPartialMatches { get; set; }
        int? MaxTokenDistance { get; set; }
    }

    public interface IPromptRecognizeChoicesOptions : IPromptRecognizeValuesOptions
    {
        bool? ExcludeValue { get; set; }
        bool? ExcludeAction { get; set; }
    }

    public class PromptRecognizeChoicesOptions : IPromptRecognizeChoicesOptions
    {
        /// <summary>
        /// (Optional) If true, the choices value will NOT be recognized over.
        /// </summary>
        public bool? ExcludeValue { get; set; }
        /// <summary>
        /// (Optional) If true, the choices action will NOT be recognized over.
        /// </summary>
        public bool? ExcludeAction { get; set; }
        /// <summary>
        /// (Optional) if true, then only some of the tokens in a value need to exist to be considered a match.The default value is "false".
        /// </summary>
        public bool? AllowPartialMatches { get; set; }
        /// <summary>
        /// (Optional) maximum tokens allowed between two matched tokens in the utterance.So with
        /// a max distance of 2 the value "second last" would match the utternace "second from the last"
        /// but it wouldn't match "Wait a second. That's not the last one is it?". 
        /// The default value is "2".  
        /// </summary>
        public int? MaxTokenDistance { get; set; }
    }

    public class ChronoDuration
    {
        public string Entity { get; internal set; }
        public ChronoDurationResolution Resolution { get; set; }

        public ChronoDuration()
        {
            this.Resolution = new ChronoDurationResolution();
        }
    }

    public class ChronoDurationResolution
    {
        public string ResolutionType { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }

    public interface IPromptRecognizer
    {
        /// <summary>Recognizer using a RegEx to match expressions.</summary>
        /// <param name="message">Message context.</param>
        /// <param name="expressionKey">Name of the resource with the RegEx.</param>
        /// <param name="resourceManager">Resources with the localized expression.</param>
        IEnumerable<RecognizeEntity<string>> RecognizeLocalizedRegExp(IMessageActivity message, string expressionKey, ResourceManager resourceManager);

        /// <summary>Recognizer for a number.</summary>
        /// <param name="message">Message context.</param>
        /// <param name="choicesDictionary">Dictionary with the options to choose from as a key and their synonyms as a value.</param>
        /// <param name="options">Options of the Recognizer. <see cref="IPromptRecognizeChoicesOptions" /></param>
        IEnumerable<RecognizeEntity<T>> RecognizeChoices<T>(IMessageActivity message, IReadOnlyDictionary<T, IReadOnlyList<T>> choicesDictionary, IPromptRecognizeChoicesOptions options = null);

        /// <summary>Recognizer for a number.</summary>
        /// <param name="message">Message context.</param>
        /// <param name="choicesKey">Name of the resource with the choices.</param>
        /// <param name="resourceManager">Resources with the localized choices.</param>
        /// <param name="options">Options of the Recognizer. <see cref="IPromptRecognizeChoicesOptions" /></param>
        IEnumerable<RecognizeEntity<string>> RecognizeLocalizedChoices(IMessageActivity message, string choicesKey, ResourceManager resourceManager, IPromptRecognizeChoicesOptions options = null);

        /// <summary>Recognizer for a number.</summary>
        /// <param name="message">Message context.</param>
        /// <param name="options">Options of the Recognizer. <see cref="IPromptRecognizeNumbersOptions" /></param>
        IEnumerable<RecognizeEntity<double>> RecognizeNumbers(IMessageActivity message, IPromptRecognizeNumbersOptions options = null);

        /// <summary>Recognizer for a ordinal number.</summary>
        /// <param name="message">Message context.</param>
        IEnumerable<RecognizeEntity<long>> RecognizeOrdinals(IMessageActivity message);

        /// <summary>Recognizer for a time or duration.</summary>
        /// <param name="message">Message context.</param>
        IEnumerable<RecognizeEntity<string>> RecognizeTimes(IMessageActivity message);

        /// <summary>Recognizer for true/false expression.</summary>
        /// <param name="message">Message context.</param>
        IEnumerable<RecognizeEntity<bool>> RecognizeBooleans(IMessageActivity message);
    }

    internal class ChoicesDictionary : Dictionary<string, IReadOnlyList<string>> { }

    internal class LocalizedDictionary<T> : ConcurrentDictionary<string, T> { }

    internal class ResourcesCache<T> : ConcurrentDictionary<string, LocalizedDictionary<T>> { }

    [Serializable]
    public class PromptRecognizer : IPromptRecognizer
    {
        private const string ResourceKeyCardinals = "NumberTerms";
        private const string ResourceKeyOrdinals = "NumberOrdinals";
        private const string ResourceKeyReverserOrdinals = "NumberReverseOrdinals";
        private const string ResourceKeyNumberRegex = "NumberExpression";
        private const string ResourceKeyBooleans = "BooleanChoices";

        private static Regex simpleTokenizer = new Regex(@"\w+", RegexOptions.IgnoreCase);
        private static ResourcesCache<Regex> expCache = new ResourcesCache<Regex>();
        private static ResourcesCache<ChoicesDictionary> choicesCache = new ResourcesCache<ChoicesDictionary>();

        public PromptRecognizer()
        {
        }

        public IEnumerable<RecognizeEntity<string>> RecognizeLocalizedRegExp(IMessageActivity message, string expressionKey, ResourceManager resourceManager)
        {
            var entities = new List<RecognizeEntity<string>>();
            var locale = message?.Locale ?? string.Empty;
            var utterance = message?.Text?.Trim().ToLowerInvariant() ?? string.Empty;

            LocalizedDictionary<Regex> cachedLocalizedRegex;
            if (!expCache.TryGetValue(expressionKey, out cachedLocalizedRegex))
            {
                var localizedRegex = new LocalizedDictionary<Regex>();
                cachedLocalizedRegex = expCache.GetOrAdd(expressionKey, localizedRegex);
            }

            Regex cachedRegex;
            if (!cachedLocalizedRegex.TryGetValue(locale, out cachedRegex))
            {
                var expression = GetLocalizedResource(expressionKey, locale, resourceManager);
                var regex = new Regex(expression, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                cachedRegex = cachedLocalizedRegex.GetOrAdd(locale, regex);
            }
            
            foreach (Match match in cachedRegex.Matches(utterance))
            {
                if (match.Success)
                {
                    entities.Add(new RecognizeEntity<string>
                    {
                        Entity = match.Value,
                        Score = CalculateScore(utterance, match.Value)
                    });
                }

            }
            return entities;
        }
        
        public IEnumerable<RecognizeEntity<string>> RecognizeLocalizedChoices(IMessageActivity message, string choicesKey, ResourceManager resourceManager, IPromptRecognizeChoicesOptions options = null)
        {
            var locale = message?.Locale ?? string.Empty;

            LocalizedDictionary<ChoicesDictionary> cachedLocalizedChoices;
            if (!choicesCache.TryGetValue(choicesKey, out cachedLocalizedChoices))
            {
                var localizedChoices = new LocalizedDictionary<ChoicesDictionary>();
                cachedLocalizedChoices = choicesCache.GetOrAdd(choicesKey, localizedChoices);
            }

            ChoicesDictionary cachedChoices;
            if (!cachedLocalizedChoices.TryGetValue(locale, out cachedChoices))
            {
                var choicesArray = GetLocalizedResource(choicesKey, locale, resourceManager).Split('|');
                var choices = ConvertToChoices(choicesArray);
                cachedChoices = cachedLocalizedChoices.GetOrAdd(locale, choices);
            }
            
            return RecognizeChoices(message, cachedChoices, options);
        }
        
        public IEnumerable<RecognizeEntity<double>> RecognizeNumbers(IMessageActivity message, IPromptRecognizeNumbersOptions options = null)
        {
            var entities = new List<RecognizeEntity<double>>();

            Func<RecognizeEntity<double>, bool> minValueWhere = (x => ((options == null || !options.MinValue.HasValue) || x.Entity >= options.MinValue));
            Func<RecognizeEntity<double>, bool> maxValueWhere = (x => ((options == null || !options.MaxValue.HasValue) || x.Entity <= options.MaxValue));
            Func<RecognizeEntity<double>, bool> integerOnlyWhere = (x => ((options != null && options.IntegerOnly.HasValue) ? !options.IntegerOnly.Value : true) || Math.Floor(x.Entity) == x.Entity);
            Func<RecognizeEntity<string>, RecognizeEntity<double>> selector = (x => new RecognizeEntity<double> { Entity = double.Parse(x.Entity), Score = x.Score });
            
            var matches = RecognizeLocalizedRegExp(message, ResourceKeyNumberRegex, Resources.ResourceManager);
            if (matches != null && matches.Any())
            {
                entities.AddRange(matches.Select(selector)
                    .Where(minValueWhere)
                    .Where(maxValueWhere)
                    .Where(integerOnlyWhere));
            }

            var resource = GetLocalizedResource(ResourceKeyCardinals, message?.Locale, Resources.ResourceManager);

            var choices = ConvertToChoices(resource.Split('|'));

            // Recognize any term based numbers
            var results = RecognizeChoices(message, choices, new PromptRecognizeChoicesOptions { ExcludeValue = true });
            if (results != null && results.Any())
            {
                entities.AddRange(results.Select(selector)
                    .Where(minValueWhere)
                    .Where(maxValueWhere)
                    .Where(integerOnlyWhere));
            }
            
            return entities;
        }

        public IEnumerable<RecognizeEntity<long>> RecognizeOrdinals(IMessageActivity message)
        {
            var entities = new List<RecognizeEntity<long>>();

            var resourceOrdinales = GetLocalizedResource(ResourceKeyOrdinals, message?.Locale, Resources.ResourceManager);
            var resourceReverseOrdinals = GetLocalizedResource(ResourceKeyReverserOrdinals, message?.Locale, Resources.ResourceManager);

            var ordinals = resourceOrdinales.Split('|');
            var reverseOrdinals = resourceReverseOrdinals.Split('|');

            var values = ordinals.Concat(reverseOrdinals);
            
            var choices = ConvertToChoices(values);
            
            // Recognize any term based numbers
            var results = RecognizeChoices(message, choices, new PromptRecognizeChoicesOptions { ExcludeValue = true });
            if (results != null && results.Any())
            {
                entities.AddRange(results.Select(x => new RecognizeEntity<long> { Entity = long.Parse(x.Entity), Score = x.Score }));
            }

            return entities;
        }

        public IEnumerable<RecognizeEntity<string>> RecognizeTimes(IMessageActivity message)
        {
            var entities = new List<RecognizeEntity<string>>();

            var utterance = message?.Text?.Trim();
            var entity = RecognizeTime(utterance);

            entities.Add(new RecognizeEntity<string>()
            {
                Entity = entity.Entity,
                Score = CalculateScore(utterance, entity.Entity)
            });

            return entities;
        }
        
        public IEnumerable<RecognizeEntity<T>> RecognizeChoices<T>(IMessageActivity message, IReadOnlyDictionary<T, IReadOnlyList<T>> choicesDictionary, IPromptRecognizeChoicesOptions options = null)
        {
            var entities = new List<RecognizeEntity<T>>();
            var index = 0;
            foreach (var choices in choicesDictionary)
            {
                var values = choices.Value?.ToList() ?? new List<T>();
                var excludeValue = options?.ExcludeValue ?? false;
                if (!excludeValue)
                {
                    values.Add(choices.Key);
                }
                var match = RecognizeValues(message, values, options).MaxBy(x => x.Score);
                if (match != null)
                {
                    entities.Add(new RecognizeEntity<T>
                    {
                        Entity = choices.Key,
                        Score = match.Score
                    });
                }
                index++;
            }
            return entities;
        }

        public IEnumerable<RecognizeEntity<bool>> RecognizeBooleans(IMessageActivity message)
        {
            var entities = new List<RecognizeEntity<bool>>();

            var results = RecognizeLocalizedChoices(message, ResourceKeyBooleans, Resources.ResourceManager, new PromptRecognizeChoicesOptions());
            if (results != null)
            {
                entities.AddRange(
                    results.Select(x => new RecognizeEntity<bool> { Entity = bool.Parse(x.Entity), Score = x.Score })
                );
            }

            return entities;
        }

        private static IEnumerable<RecognizeEntity<T>> RecognizeValues<T>(IMessageActivity message, IEnumerable<T> values, IPromptRecognizeChoicesOptions options = null)
        {
            var utterance = message?.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            var entities = new List<RecognizeEntity<T>>();
            IList<string> tokens = new List<string>();
            foreach (Match match in simpleTokenizer.Matches(utterance))
            {
                tokens.Add(match.Value);
            }
            var maxDistance = options?.MaxTokenDistance ?? 2;
            var index = 0;
            foreach (var value in values)
            {
                var text = value?.ToString().Trim().ToLowerInvariant();
                var topScore = 0.0;
                IList<string> vTokens = new List<string>();
                foreach (Match match in simpleTokenizer.Matches(text))
                {
                    vTokens.Add(match.Value);
                }
                for (int i = 0; i < tokens.Count; i++)
                {
                    var score = MatchValue(tokens.ToArray(), vTokens.ToArray(), i, maxDistance, options?.AllowPartialMatches ?? false);
                    if (topScore < score)
                    {
                        topScore = score;
                    }
                }
                if (topScore > 0.0)
                {
                    entities.Add(new RecognizeEntity<T>
                    {
                        Entity = value,
                        Score = topScore
                    });
                }
                index++;
            }
            return entities;
        }

        private static ChronoDuration RecognizeTime(string utterance)
        {
            ChronoDuration response = null;
            try
            {
                Chronic.Parser parser = new Chronic.Parser();
                var results = parser.Parse(utterance);

                if (results != null)
                {
                    response = new ChronoDuration()
                    {
                        Entity = results.ToTime().TimeOfDay.ToString(),
                        Resolution = new ChronoDurationResolution()
                        {
                            Start = results.Start,
                            End = results.End
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recognizing time: {ex.Message}");
                response = null;
            }

            return response;
        }

        private static ChoicesDictionary ConvertToChoices(IEnumerable<string> values)
        {
            var result = new ChoicesDictionary();
            foreach (var term in values)
            {
                var subTerm = term.Split('=');
                if (subTerm.Count() == 2)
                {
                    var choices = subTerm[1].Split(',');
                    result.Add(subTerm[0], choices);
                }
                else
                {
                    result.Add(subTerm[0], Enumerable.Empty<string>().ToList());
                }
            }
            return result;
        }


        private static double MatchValue(string[] tokens, string[] vTokens, int index, int maxDistance, bool allowPartialMatches)
        {
            var startPosition = index;
            double matched = 0;
            var totalDeviation = 0;
            foreach (var token in vTokens)
            {
                var pos = IndexOfToken(tokens.ToList(), token, startPosition);
                if (pos >= 0)
                {
                    var distance = matched > 0 ? pos - startPosition : 0;
                    if (distance <= maxDistance)
                    {
                        matched++;
                        totalDeviation += distance;
                        startPosition = pos + 1;
                    }
                }
            }

            var score = 0.0;
            if (matched > 0 && (matched == vTokens.Length || allowPartialMatches))
            {
                var completeness = matched / vTokens.Length;
                var accuracy = completeness * (matched / (matched + totalDeviation));
                var initialScore = accuracy * (matched / tokens.Length);

                score = 0.4 + (0.6 * initialScore);
            }
            return score;
        }

        private static int IndexOfToken(List<string> tokens, string token, int startPos)
        {
            if (tokens.Count <= startPos) return -1;
            return tokens.FindIndex(startPos, x => x == token);
        }

        private static double CalculateScore(string utterance, string entity, double max = 1.0, double min = 0.5)
        {
            return Math.Min(min + (entity.Length / (double)utterance.Length), max);
        }

        private static string GetLocalizedResource(string resourceKey, string locale, ResourceManager resourceManager)
        {
            CultureInfo culture;
            try
            {
                culture = new CultureInfo(locale);
            }
            catch
            {
                culture = new CultureInfo("en-US");
            }
            return resourceManager.GetString(resourceKey, culture);
        }
    }

    public static partial class Extensions
    {
        /// <summary>Recognizer for a Int64 number.</summary>
        /// <param name="recognizer"><see cref="IPromptRecognizer"/></param>
        /// <param name="message">Message context.</param>
        public static IEnumerable<RecognizeEntity<Int64>> RecognizeInteger(this IPromptRecognizer recognizer, IMessageActivity message)
        {
            var entities = recognizer.RecognizeNumbers(message, new PromptRecognizeNumbersOptions { IntegerOnly = true });
            return entities.Select(x => new RecognizeEntity<Int64> { Entity = Convert.ToInt64(x.Entity), Score = x.Score });
        }

        /// <summary>Recognizer for a double number.</summary>
        /// <param name="recognizer"><see cref="IPromptRecognizer"/></param>
        /// <param name="message">Message context.</param>
        public static IEnumerable<RecognizeEntity<double>> RecognizeDouble(this IPromptRecognizer recognizer, IMessageActivity message)
        {
            return recognizer.RecognizeNumbers(message, new PromptRecognizeNumbersOptions { IntegerOnly = false });
        }

        /// <summary>Recognizer for a Int64 number within a range</summary>
        /// <param name="recognizer"><see cref="IPromptRecognizer"/></param>
        /// <param name="message">Message context.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="min">Minimun value.</param>
        public static IEnumerable<RecognizeEntity<Int64>> RecognizeIntegerInRange(this IPromptRecognizer recognizer, IMessageActivity message, long? min, long? max)
        {
            var entities = recognizer.RecognizeNumbers(message, new PromptRecognizeNumbersOptions { IntegerOnly = true, MinValue = min, MaxValue = max });
            return entities.Select(x => new RecognizeEntity<Int64> { Entity = Convert.ToInt64(x.Entity), Score = x.Score });
        }

        /// <summary>Recognizes the double in range.</summary>
        /// <param name="recognizer"><see cref="IPromptRecognizer"/></param>
        /// <param name="message">Message context.</param>
        /// <param name="max">Maximum value.</param>
        /// <param name="min">Minimun value.</param>
        public static IEnumerable<RecognizeEntity<double>> RecognizeDoubleInRange(this IPromptRecognizer recognizer, IMessageActivity message, double? min, double? max)
        {
            var entities = recognizer.RecognizeNumbers(message, new PromptRecognizeNumbersOptions { IntegerOnly = false, MinValue = min, MaxValue = max });
            return entities.Select(x => new RecognizeEntity<double> { Entity = Convert.ToDouble(x.Entity), Score = x.Score });
        }
    }
}