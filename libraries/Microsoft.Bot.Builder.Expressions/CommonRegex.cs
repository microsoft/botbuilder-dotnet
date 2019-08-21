using System;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.Expressions
{
    public class CommonRegex
    {
        private static readonly LRUCache<string, Regex> RegexCache = new LRUCache<string, Regex>(15);

        public static Regex CreateRegex(string pattern)
        {
            Regex result;
            if (!string.IsNullOrEmpty(pattern) && RegexCache.TryGet(pattern, out var regex))
            {
                result = regex;
            }
            else
            {
                if (string.IsNullOrEmpty(pattern) || !IsCommonRegex(pattern))
                {
                    throw new ArgumentException("A regular expression parsing error occurred.");
                }

                result = new Regex(pattern, RegexOptions.Compiled);
                RegexCache.Set(pattern, result);
            }

            return result;
        }

        private static bool IsCommonRegex(string pattern)
        {
            try
            {
                AntlrParse(pattern);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static IParseTree AntlrParse(string pattern)
        {
            var inputStream = new AntlrInputStream(pattern);
            var lexer = new CommonRegexLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new CommonRegexParser(tokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(new ErrorListener());
            parser.BuildParseTree = true;

            return parser.parse();
        }
    }

    internal class ErrorListener : BaseErrorListener
    {
        public static readonly ErrorListener Instance = new ErrorListener();

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e) => throw new Exception($"Regular expression is invalid.");
    }
}
