using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.AI.LanguageGeneration
{
    public class TemplateErrorListener : BaseErrorListener
    {
        private List<LGReportMessage> ParseExceptions;
        public TemplateErrorListener()
        {
            ParseExceptions = new List<LGReportMessage>();
        }
        
        public List<LGReportMessage>  GetExceptions()
        {
            return ParseExceptions;
        }

        public override void SyntaxError([NotNull] Antlr4.Runtime.IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            ParseExceptions.Add(new LGReportMessage($"line {line}:{charPositionInLine} {msg}"));
        }
    }
}
