using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.Parsers.LU
{
    internal class LUErrorListener : BaseErrorListener
    {
        private readonly List<Error> _errors;

        public LUErrorListener(List<Error> errors)
        {
            this._errors = errors;
        }

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var startPosition = new Position { Line = line, Character = charPositionInLine };
            var stopPosition = new Position { Line = line, Character = charPositionInLine + offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1 };
            var range = new Range { Start = startPosition, End = stopPosition };
            msg = "syntax error: " + msg;

            //var invalidToken = msg.match(/ '([^']+)'/)[1];
            //const expectedTokenStr = msg.substring(msg.indexOf('{') + 1, msg.lastIndexOf('}'));
            //const expectedTokens = expectedTokenStr.split(',');
            //if (expectedTokenStr.length > 0 && expectedTokens.length > 0)
            //{
            //    msg = `syntax error: invalid input '${invalidToken}' detected.Expecting one of this - `;
            //    expectedTokens.forEach(token => {
            //        msg += AntlrTokens[token.trim()] + ', ';
            //    });

            //    msg = msg.substring(0, msg.lastIndexOf(', '));
            //}

            this._errors.Add(new Diagnostic { Message = msg, Range = range });
        }
    }
}
