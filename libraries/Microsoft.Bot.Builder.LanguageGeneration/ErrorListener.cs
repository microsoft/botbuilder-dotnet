using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class ErrorListener : BaseErrorListener
    {
        private readonly string source;

        public ErrorListener(string errorSource)
        {
            source = errorSource;
        }

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            var startPosition = new Position(line, charPositionInLine);
            var stopPosition = new Position(line, charPositionInLine + offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1);
            var range = new Range(startPosition, stopPosition);
            msg = $"source: {source}. syntax error message: {msg}";
            var diagnostic = new Diagnostic(range, msg, DiagnosticSeverity.Error);
            throw new LGException(diagnostic.ToString(), new List<Diagnostic>() { diagnostic });
        }
    }
}
