using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chronic
{
    public class OrdinalScanner : ITokenScanner
    {
        protected static readonly Regex Pattern = new Regex(
            @"^(\d*)(st|nd|rd|th)$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        public IList<Token> Scan(IList<Token> tokens, Options options)
        {
            tokens.ForEach(token => token.Tag(
                new ITag[]
                    {
                        ScanOrdinal(token, options),
                        ScanOrdinalDay(token)
                    }.Where(
                        x => x != null).ToList()));
            return tokens;
        }

        public Ordinal ScanOrdinal(Token token, Options options)
        {
            var match = Pattern.Match(token.Value);

            if (match.Success)
            {
                return new Ordinal(int.Parse(match.Groups[1].Value));
            }
            return null;
        }

        public OrdinalDay ScanOrdinalDay(Token token)
        {
            var match = Pattern.Match(token.Value);

            if (match.Success)
            {
                var value = int.Parse(match.Groups[1].Value);
                if (value <= 31)
                    return new OrdinalDay(value);
            }
            return null;
        }

    }
}