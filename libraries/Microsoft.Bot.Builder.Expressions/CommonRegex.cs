using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Builder.Expressions
{
    public class CommonRegex
    {
        public static Regex CreateRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern) || !IsCommonRegex(pattern))
            {
                throw new ArgumentException("A regular expression parsing error occurred.");
            }

            // TODO check pattern
            return new Regex(pattern);
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
