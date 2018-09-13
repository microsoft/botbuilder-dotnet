using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Chronic
{
    public class ScalarScanner : ITokenScanner
    {
        static readonly Regex _pattern = new Regex(
            @"^\d*$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _dayPattern = new Regex(
            @"^\d\d?$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly Regex _monthPattern = _dayPattern;
        static readonly Regex _yearPattern = new Regex(
            @"^([1-9]\d)?\d\d?$",
            RegexOptions.Singleline | RegexOptions.Compiled);

        static readonly string[] _dayPeriods = new string[] { "am", "pm", "morning", "afternoon", "evening", "night" };


        public IList<Token> Scan(IList<Token> tokens, Options options)
        {
            tokens.ForEach((token, nextToken) => token.Tag(
                new ITag[]
                    {
                        Scan(token, nextToken, options),
                        ScanDay(token, nextToken, options),
                        ScanMonth(token, nextToken, options),
                        ScanYear(token, nextToken, options),
                    }.Where(
                        x => x != null).ToList()));
            return tokens;
        }

        public static Scalar Scan(Token token, Token nextToken, Options options)
        {
            var match = _pattern.Match(token.Value);

            if (match.Success && String.IsNullOrEmpty(token.Value) == false
                && TokenIsAPeriodOfDay(nextToken) == false)
            {
                return new Scalar(int.Parse(match.Groups[0].Value));
            }
            return null;
        }

        public static Scalar ScanDay(Token token, Token nextToken, Options options)
        {
            if (_dayPattern.IsMatch(token.Value))
            {
                var value = int.Parse(token.Value);
                if (value <= 31 && TokenIsAPeriodOfDay(nextToken) == false)
                    return new ScalarDay(value);
            }
            return null;
        }

        public static Scalar ScanMonth(Token token, Token nextToken, Options options)
        {
            if (_monthPattern.IsMatch(token.Value))
            {
                var value = int.Parse(token.Value);
                if (value <= 12 && TokenIsAPeriodOfDay(nextToken) == false)
                    return new ScalarMonth(value);
            }
            return null;
        }

        public static Scalar ScanYear(Token token, Token nextToken, Options options)
        {
            if (_yearPattern.IsMatch(token.Value))
            {
                var value = int.Parse(token.Value);
                if (TokenIsAPeriodOfDay(nextToken) == false)
                {
                    if (value <= 37)
                    {
                        value += 2000;
                    }
                    else if (value <= 137 && value >= 69)
                    {
                        value += 1900;
                    }
                    return new ScalarYear(value);
                }
            }
            return null;
        }

        static bool TokenIsAPeriodOfDay(Token token)
        {
            return token != null && _dayPeriods.Contains(token.Value);
        }
    }
}