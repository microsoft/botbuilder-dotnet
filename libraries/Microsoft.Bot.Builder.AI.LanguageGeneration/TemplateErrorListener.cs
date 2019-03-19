using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class TemplateErrorListener : BaseErrorListener
    {
        public static readonly TemplateErrorListener Instance = new TemplateErrorListener();

        public override void SyntaxError([NotNull] Antlr4.Runtime.IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new Exception($"line {line}:{charPositionInLine} {msg}");
        }
    }
}
