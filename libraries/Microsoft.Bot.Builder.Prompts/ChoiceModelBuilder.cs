using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.Choice;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Definitons = Microsoft.Recognizers.Definitions;

namespace Microsoft.Bot.Builder.Prompts
{
    /// <summary>
    /// ChoiceModelBuilder generates a <see cref="ChoiceModel"/> recognizer
    /// using a dictionary with the values to match and a <typeparamref name="T"/> result.
    /// </summary>
    public class ChoiceModelBuilder<T>
    {
        private IDictionary<Regex, T> mapResolutions;
        private Regex tokenRegex;
        private bool allowPartialMatch = false;
        private int maxDistance = 2;

        /// <summary>
        /// Creates a <see cref="ChoiceModelBuilder{T}"/> object.
        /// </summary>
        /// <param name="mapResolutions">A dictionary with a regex to match and the recognized value for each regex.</param>
        public ChoiceModelBuilder(IDictionary<Regex, T> mapResolutions)
        {
            this.mapResolutions = mapResolutions;
        }

        /// <summary>
        /// Creates a <see cref="ChoiceModelBuilder{T}"/> object.
        /// </summary>
        /// <param name="mapResolutions">A dictionary with a list of possible values and the recognized value the each list.</param>
        public ChoiceModelBuilder(IDictionary<IEnumerable<string>, T> mapResolutions, bool wholeWordsOnly = false, bool ignoreCase = false)
        {
            this.mapResolutions = new Dictionary<Regex, T>();

            var (preffix, suffix) = wholeWordsOnly ? ("\b(", ")\b") : ("(", ")");
            var regexFlags = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            foreach (var item in mapResolutions)
            {
                var pattern = string.Join("|", item.Key.OrderByDescending(x => x.Length).Select(x => Regex.Escape(x)));
                var regex = new Regex($"{preffix}{pattern}{suffix}", regexFlags);
                this.mapResolutions.Add(regex, item.Value);
            }
        }

        /// <summary>
        /// Set the regex used to separate the user text into tokens
        /// </summary>
        public ChoiceModelBuilder<T> WithTokenRegex(Regex tokenRegex)
        {
            this.tokenRegex = tokenRegex;
            return this;
        }

        /// <summary>
        /// If true, then only some of the words in a value need to exist to be considered a match. The default value is "false".
        /// </summary>
        public ChoiceModelBuilder<T> WithAllowPartialMatch(bool allowPartialMatch)
        {
            this.allowPartialMatch = allowPartialMatch;
            return this;
        }

        /// <summary>
        /// Maximum words allowed between two matched words in the utterance.
        /// </summary>
        public ChoiceModelBuilder<T> WithMaxDistance(int maxDistance)
        {
            this.maxDistance = maxDistance;
            return this;
        }

        /// <summary>
        /// Set the culture that defines which regex will be used to separate the user text into tokens
        /// </summary>
        /// <remarks>If <paramref name="culture"/> is defined, the internal regex used to
        /// separate the user's input text will be overwritten.
        /// </remarks>
        public ChoiceModelBuilder<T> WithCulture(string culture)
        {
            var tokenPattern = GetTokenPatternFromCulture(culture);
            tokenRegex = new Regex(tokenPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return this;
        }
        
        /// <summary>
        /// Build a <see cref="ChoiceModel"/>
        /// </summary>
        public IModel Build()
        {
            if (tokenRegex == null)
            {
                tokenRegex = new Regex(Definitons.English.ChoiceDefinitions.TokenizerRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }
            return BaseModel<T>.CreateFromChoices(mapResolutions, tokenRegex, allowPartialMatch, maxDistance);
        }

        private string GetTokenPatternFromCulture(string culture)
        {
            switch (culture)
            {
                case Culture.English:
                    return Definitons.English.ChoiceDefinitions.TokenizerRegex;
                case Culture.Portuguese:
                    return Definitons.Portuguese.ChoiceDefinitions.TokenizerRegex;
                case Culture.Japanese:
                    return Definitons.Japanese.ChoiceDefinitions.TokenizerRegex;
                case Culture.Spanish:
                    return Definitons.Spanish.ChoiceDefinitions.TokenizerRegex;
            }
            return Definitons.English.ChoiceDefinitions.TokenizerRegex;
        }
    }

    internal class BaseExtractorConfiguration : IChoiceExtractorConfiguration
    {
        public IDictionary<Regex, string> MapRegexes { get; private set; }
        public Regex TokenRegex { get; private set; }
        public bool AllowPartialMatch { get; private set; }
        public int MaxDistance { get; private set; }
        public bool OnlyTopMatch => true;

        public BaseExtractorConfiguration(IDictionary<Regex, string> mapRegex, Regex tokenRegex, bool allowPartialMatch, int maxDistance)
        {
            MapRegexes = mapRegex;
            TokenRegex = tokenRegex;
            AllowPartialMatch = AllowPartialMatch;
            MaxDistance = maxDistance;
        }
    }

    internal class BaseParserConfiguration<T> : IChoiceParserConfiguration<T>
    {
        public IDictionary<string, T> Resolutions { get; private set; }

        public BaseParserConfiguration(IDictionary<string, T> resolutions)
        {
            Resolutions = resolutions;
        }
    }

    internal class BaseModel<T> : ChoiceModel
    {
        public override string ModelTypeName => "custom";

        public BaseModel(OptionsParser<T> parser, ChoiceExtractor extractor)
            : base(parser, extractor)
        {
        }

        protected override SortedDictionary<string, object> GetResolution(ParseResult parseResult)
        {
            var data = parseResult.Data as OptionsParseDataResult;
            return new SortedDictionary<string, object>()
            {
                { "value", parseResult.Value },
                { "score", data.Score },
                { "otherResults", data.OtherMatches.Select(l => new { l.Text, l.Value, l.Score })},
            };
        }

        public static BaseModel<T> CreateFromChoices(IDictionary<Regex, T> mapResolutions, Regex tokenRegex, bool allowPartialMatch, int maxDistance)
        {
            var mapRegexes = new Dictionary<Regex, string>();
            var resolutions = new Dictionary<string, T>();
            var index = 0;
            foreach(var regex_resolution in mapResolutions)
            {
                var mappedValue = $"value{index++}";
                mapRegexes.Add(regex_resolution.Key, mappedValue);
                resolutions.Add(mappedValue, regex_resolution.Value);
            }
            var extractorConfig = new BaseExtractorConfiguration(mapRegexes, tokenRegex, allowPartialMatch, maxDistance);
            var parserConfig = new BaseParserConfiguration<T>(resolutions);
            return new BaseModel<T>(new OptionsParser<T>(parserConfig), new ChoiceExtractor(extractorConfig));
        }
    }
}
