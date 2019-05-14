using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class ErrorListener : BaseErrorListener
    {
        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            var startPosition = new Position(line, charPositionInLine);
            var stopPosition = new Position(line, charPositionInLine + offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1);
            var range = new Range(startPosition, stopPosition);
            msg = "syntax error at " + msg;
            var diagnostic = new Diagnostic(range, msg, DiagnosticSeverity.Error);

            throw new Exception(JsonConvert.SerializeObject(diagnostic));
        }
    }
}
