using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Expressions
{
    public class ExpressionErrorListener : BaseErrorListener
    {
        public static readonly ExpressionErrorListener Instance = new ExpressionErrorListener();

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new Exception($"line {line}:{charPositionInLine} {msg}");
            throw new Exception("line " + line + ":" + charPositionInLine + " " + msg);
        }
    }
}
