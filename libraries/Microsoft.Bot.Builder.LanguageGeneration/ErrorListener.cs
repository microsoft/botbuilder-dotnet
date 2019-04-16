using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class ErrorListener : BaseErrorListener
    {
        public override void SyntaxError([NotNull] Antlr4.Runtime.IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            var lineStrArray = offendingSymbol.InputStream.GetText(new Interval(0, int.MaxValue)).Split('\n');
            var lineStr = lineStrArray[line - 1];

            var errorbuilder = new StringBuilder();
            errorbuilder.Append("[ERROR]:\r\n");
            errorbuilder.Append(lineStr + "\r\n");
            errorbuilder.Append(new string(' ', charPositionInLine));

            var length = offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1;
            errorbuilder.Append(new string('^', length));
            errorbuilder.Append("\r\n");
            errorbuilder.Append($"{msg}\r\n");
            throw new Exception(errorbuilder.ToString());
        }
    }
}
