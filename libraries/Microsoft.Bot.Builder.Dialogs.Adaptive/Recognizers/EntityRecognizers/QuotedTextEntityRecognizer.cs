using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Recognizers.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers
{
    /// <summary>
    /// Matches quoted text as entity.
    /// </summary>
    public class QuotedTextEntityRecognizer : TextEntityRecognizer
    {
        /// <summary>
        /// Class identifier.
        /// </summary>
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.QuotedTextEntityRecognizer";

        /// <summary>
        /// Entity name for quoted text.
        /// </summary>
        public const string QuotedText = "QuotedText";

        // from https://en.wikipedia.org/wiki/Quotation_mark
        private Dictionary<string, LanguageQuotePolicy> _policies = new Dictionary<string, LanguageQuotePolicy>()
        {
            { "af", new LanguageQuotePolicy { Language = "Afrikaans", Code = "af", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('„', '”'), new QuotePattern('‚', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "sq", new LanguageQuotePolicy { Language = "Albanian", Code = "sq", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "am", new LanguageQuotePolicy { Language = "Amharic", Code = "am", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ar", new LanguageQuotePolicy { Language = "Arabic", Code = "ar", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('”', '“'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "hy", new LanguageQuotePolicy { Language = "Armenian", Code = "hy", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "az", new LanguageQuotePolicy { Language = "Azerbaijani", Code = "az", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('„', '“'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "eu", new LanguageQuotePolicy { Language = "Basque", Code = "eu", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "be", new LanguageQuotePolicy { Language = "Belarusian", Code = "be", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "bs", new LanguageQuotePolicy { Language = "Bosnian", Code = "bs", Patterns = new List<QuotePattern>() { new QuotePattern('”', '”'), new QuotePattern('’', '’'), new QuotePattern('„', '“'), new QuotePattern('»', '«'), new QuotePattern('„', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "bg", new LanguageQuotePolicy { Language = "Bulgarian", Code = "bg", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('’', '’'), new QuotePattern('«', '»'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ca", new LanguageQuotePolicy { Language = "Catalan", Code = "ca", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "zh", new LanguageQuotePolicy { Language = "Chinese", Code = "zh", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('『', '』'), new QuotePattern('「', '」'), new QuotePattern('﹃', '﹄'), new QuotePattern('﹁', '﹂'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "hr", new LanguageQuotePolicy { Language = "Croatian", Code = "hr", Patterns = new List<QuotePattern>() { new QuotePattern('„', '”'), new QuotePattern('‘', '’'), new QuotePattern('»', '«'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "cs", new LanguageQuotePolicy { Language = "Czech", Code = "cs", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('»', '«'), new QuotePattern('›', '‹'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "da", new LanguageQuotePolicy { Language = "Danish", Code = "da", Patterns = new List<QuotePattern>() { new QuotePattern('»', '«'), new QuotePattern('›', '‹'), new QuotePattern('”', '”'), new QuotePattern('’', '’'), new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "nl", new LanguageQuotePolicy { Language = "Dutch", Code = "nl", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('„', '”'), new QuotePattern(',', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "en", new LanguageQuotePolicy { Language = "English", Code = "en", Patterns = new List<QuotePattern>() { new QuotePattern('‘', '’'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "eo", new LanguageQuotePolicy { Language = "Esperanto", Code = "eo", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "et", new LanguageQuotePolicy { Language = "Estonian", Code = "et", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "fil", new LanguageQuotePolicy { Language = "Filipino", Code = "fil", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "fi", new LanguageQuotePolicy { Language = "Finnish", Code = "fi", Patterns = new List<QuotePattern>() { new QuotePattern('”', '”'), new QuotePattern('’', '’'), new QuotePattern('»', '»'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "fr", new LanguageQuotePolicy { Language = "French", Code = "fr", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "gl", new LanguageQuotePolicy { Language = "Galician", Code = "gl", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ka", new LanguageQuotePolicy { Language = "Georgian", Code = "ka", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "de", new LanguageQuotePolicy { Language = "German", Code = "de", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('»', '«'), new QuotePattern('›', '‹'), new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "el", new LanguageQuotePolicy { Language = "Greek", Code = "el", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "he", new LanguageQuotePolicy { Language = "Hebrew", Code = "he", Patterns = new List<QuotePattern>() { new QuotePattern('”', '”'), new QuotePattern('’', '’'), new QuotePattern('„', '”'), new QuotePattern('‚', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "hi", new LanguageQuotePolicy { Language = "Hindi", Code = "hi", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "hu", new LanguageQuotePolicy { Language = "Hungarian", Code = "hu", Patterns = new List<QuotePattern>() { new QuotePattern('„', '”'), new QuotePattern('»', '«'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "is", new LanguageQuotePolicy { Language = "Icelandic", Code = "is", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "id", new LanguageQuotePolicy { Language = "Indonesian", Code = "id", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ia", new LanguageQuotePolicy { Language = "Interlingua", Code = "ia", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ga", new LanguageQuotePolicy { Language = "Irish", Code = "ga", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "it", new LanguageQuotePolicy { Language = "Italian", Code = "it", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('‹', '›'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ja", new LanguageQuotePolicy { Language = "Japanese", Code = "ja", Patterns = new List<QuotePattern>() { new QuotePattern('「', '」'), new QuotePattern('『', '』'), new QuotePattern('﹁', '⋮'), new QuotePattern('﹃', '﹄'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "kk", new LanguageQuotePolicy { Language = "Kazakh", Code = "kk", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "km", new LanguageQuotePolicy { Language = "Khmer", Code = "km", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ko", new LanguageQuotePolicy { Language = "Korean", Code = "ko", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('﹃', '﹄'), new QuotePattern('﹁', '﹂'), new QuotePattern('《', '》'), new QuotePattern('〈', '〉'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "lo", new LanguageQuotePolicy { Language = "Lao", Code = "lo", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "lv", new LanguageQuotePolicy { Language = "Latvian", Code = "lv", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('„', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "lt", new LanguageQuotePolicy { Language = "Lithuanian", Code = "lt", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "mk", new LanguageQuotePolicy { Language = "Macedonian", Code = "mk", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('’', '‘'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "mt", new LanguageQuotePolicy { Language = "Maltese", Code = "mt", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('«', '»'), new QuotePattern('„', '“'), new QuotePattern('《', '》'), new QuotePattern('〈', '〉'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "no", new LanguageQuotePolicy { Language = "Norwegian", Code = "no", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('‘', '’'), new QuotePattern('„', '“'), new QuotePattern(',', '‘'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "oc", new LanguageQuotePolicy { Language = "Occitan", Code = "oc", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ps", new LanguageQuotePolicy { Language = "Pashto", Code = "ps", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "fa", new LanguageQuotePolicy { Language = "Persian", Code = "fa", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "pl", new LanguageQuotePolicy { Language = "Polish", Code = "pl", Patterns = new List<QuotePattern>() { new QuotePattern('„', '”'), new QuotePattern('«', '»'), new QuotePattern('»', '«'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ro", new LanguageQuotePolicy { Language = "Romanian", Code = "ro", Patterns = new List<QuotePattern>() { new QuotePattern('„', '”'), new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ru", new LanguageQuotePolicy { Language = "Russian", Code = "ru", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('„', '“'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "sr", new LanguageQuotePolicy { Language = "Serbian", Code = "sr", Patterns = new List<QuotePattern>() { new QuotePattern('„', '”'), new QuotePattern('’', '’'), new QuotePattern('„', '“'), new QuotePattern('«', '»'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "gd", new LanguageQuotePolicy { Language = "Scottish Gaelic", Code = "gd", Patterns = new List<QuotePattern>() { new QuotePattern('‘', '’'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "sk", new LanguageQuotePolicy { Language = "Slovak", Code = "sk", Patterns = new List<QuotePattern>() { new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('»', '«'), new QuotePattern('›', '‹'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "es", new LanguageQuotePolicy { Language = "Spanish", Code = "es", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "sv", new LanguageQuotePolicy { Language = "Swedish", Code = "sv", Patterns = new List<QuotePattern>() { new QuotePattern('”', '”'), new QuotePattern('’', '’'), new QuotePattern('»', '»'), new QuotePattern('»', '«'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ta", new LanguageQuotePolicy { Language = "Tamil", Code = "ta", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "bo", new LanguageQuotePolicy { Language = "Tibetan", Code = "bo", Patterns = new List<QuotePattern>() { new QuotePattern('《', '》'), new QuotePattern('〈', '〉'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ti", new LanguageQuotePolicy { Language = "Tigrinya", Code = "ti", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "th", new LanguageQuotePolicy { Language = "Thai", Code = "th", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "tr", new LanguageQuotePolicy { Language = "Turkish", Code = "tr", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "uk", new LanguageQuotePolicy { Language = "Ukrainian", Code = "uk", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('“', '”'), new QuotePattern('‘', '’'), new QuotePattern('„', '“'), new QuotePattern('‘', '’'), new QuotePattern('„', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "ug", new LanguageQuotePolicy { Language = "Uyghur", Code = "ug", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('‹', '›'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "uz", new LanguageQuotePolicy { Language = "Uzbek", Code = "uz", Patterns = new List<QuotePattern>() { new QuotePattern('«', '»'), new QuotePattern('„', '“'), new QuotePattern('‚', '‘'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "vi", new LanguageQuotePolicy { Language = "Vietnamese", Code = "vi", Patterns = new List<QuotePattern>() { new QuotePattern('“', '”'), new QuotePattern('«', '»'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
            { "cy", new LanguageQuotePolicy { Language = "Welsh", Code = "cy", Patterns = new List<QuotePattern>() { new QuotePattern('‘', '’'), new QuotePattern('“', '”'), new QuotePattern('"', '"'), new QuotePattern('\'', '\''), new QuotePattern('`', '`') } } },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="QuotedTextEntityRecognizer"/> class.
        /// </summary>
        [JsonConstructor]
        public QuotedTextEntityRecognizer()
        {
        }

        /// <summary>
        /// Match recognizing implementation.
        /// </summary>
        /// <param name="text">Text to match.</param>
        /// <param name="culture"><see cref="Culture"/> to use.</param>
        /// <returns>The matched <see cref="ModelResult"/> list.</returns>
        protected override List<ModelResult> Recognize(string text, string culture)
        {
            var locale = string.IsNullOrEmpty(culture) ? Thread.CurrentThread.CurrentCulture.Name : culture;
            locale = locale.Split('-').FirstOrDefault();
            if (_policies.TryGetValue(locale.Split('-').FirstOrDefault(), out var policy))
            {
                return policy.Matches(text);
            }

            return new List<ModelResult>();
        }

        private class LanguageQuotePolicy
        {
            public string Language { get; set; }

            public string Code { get; set; }

            public List<QuotePattern> Patterns { get; set; } = new List<QuotePattern>();

            public List<ModelResult> Matches(string text)
            {
                var results = new List<ModelResult>();

                foreach (var pattern in Patterns)
                {
                    var matches = pattern.Matches(text);
                    if (matches.Any())
                    {
                        results.AddRange(matches);
                    }
                }

                return results;
            }
        }

        private class QuotePattern
        {
            public QuotePattern(char prefix, char postfix)
            {
                Prefix = prefix;
                Postfix = postfix;
            }

            public char Prefix { get; set; }

            public char Postfix { get; set; }

            public IEnumerable<ModelResult> Matches(string text)
            {
                List<ModelResult> matches = new List<ModelResult>();
                StringBuilder sb = new StringBuilder();
                int quoteStart = -1;
                for (int index = 0; index < text.Length; index++)
                {
                    char ch = text[index];
                    if (quoteStart < 0)
                    {
                        if (ch == Prefix)
                        {
                            // skip quote char
                            quoteStart = index + 1;
                        }
                    }
                    else
                    {
                        if (ch == Postfix)
                        {
                            matches.Add(new ModelResult()
                            {
                                TypeName = QuotedTextEntityRecognizer.QuotedText,
                                Start = quoteStart,
                                End = index,
                                Text = sb.ToString(),
                            });
                            sb.Clear();
                            quoteStart = -1;
                        }
                        else
                        {
                            sb.Append(ch);
                        }
                    }
                }

                return matches;
            }
        }
    }
}
