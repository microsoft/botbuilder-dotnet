// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
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

            this._errors.Add(new Diagnostic { Message = msg, Range = range });
        }
    }
}
