using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class TemplateErrorListener : BaseErrorListener
    {
        public override void SyntaxError([NotNull] Antlr4.Runtime.IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new Exception($"syntax error at line {line}:{charPositionInLine} {msg}");
        }
    }
}
