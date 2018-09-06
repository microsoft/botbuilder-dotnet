using System;
using System.Collections.Generic;
using System.Linq;
using Chronic.Tags.Repeaters;

namespace Chronic
{
    public class Tokenizer
    {
        static readonly List<ITokenScanner> _scanners = new List<ITokenScanner>
        {
            new RepeaterScanner(),
            new GrabberScanner(),
            new PointerScanner(),
            new ScalarScanner(), 
            new OrdinalScanner(), 
            new SeparatorScanner(),
            new TimeZoneScanner(),
        };

        IList<Token> TokenizeInternal(string phrase, Options options)
        {
            var tokens = phrase
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => new Token(part))
                .ToList();
            return tokens;
        }

        public IList<Token> Tokenize(string phrase, Options options)
        {
            options.OriginalPhrase = phrase;
            Logger.Log(() => phrase);

            phrase = Normalize(phrase);
            Logger.Log(() => phrase);

            var tokens = TokenizeInternal(phrase, options);
            _scanners.ForEach(scanner => scanner.Scan(tokens, options));
            var taggedTokens = tokens.Where(token => token.HasTags()).ToList();
            Logger.Log(() => String.Join(",", taggedTokens.Select(t => t.ToString())));

            return taggedTokens;
        }

        public static string Normalize(string phrase)
        {
            var normalized = phrase.ToLower();
            normalized = normalized
                .ReplaceAll(@"([/\-,@])", " " + "$1" + " ")
                .ReplaceAll(@"['""\.,]", "")
                .ReplaceAll(@"\bsecond (of|day|month|hour|minute|second)\b", "2nd $1")
                .Numerize()
                .ReplaceAll(@" \-(\d{4})\b", " tzminus$1")
                .ReplaceAll(@"(?:^|\s)0(\d+:\d+\s*pm?\b)", "$1")
                .ReplaceAll(@"\btoday\b", "this day")
                .ReplaceAll(@"\btomm?orr?ow\b", "next day")
                .ReplaceAll(@"\byesterday\b", "last day")
                .ReplaceAll(@"\bnoon\b", "12:00")
                .ReplaceAll(@"\bmidnight\b", "24:00")
                .ReplaceAll(@"\bbefore now\b", "past")
                .ReplaceAll(@"\bnow\b", "this second")
                .ReplaceAll(@"\b(ago|before)\b", "past")
                .ReplaceAll(@"\bthis past\b", "last")
                .ReplaceAll(@"\bthis last\b", "last")
                .ReplaceAll(@"\b(?:in|during) the (morning)\b", "$1")
                .ReplaceAll(@"\b(?:in the|during the|at) (afternoon|evening|night)\b", "$1")
                .ReplaceAll(@"\btonight\b", "this night")
                .ReplaceAll(@"(\d)([ap]m|oclock)\b", "$1 $2")
                .ReplaceAll(@"\b(hence|after|from)\b", "future")
                ;

            return normalized;
        }
    }
}