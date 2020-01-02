// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

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
            var diagnostic = new Diagnostic(range, msg, DiagnosticSeverity.Error, source);
            throw new LGException(diagnostic.ToString(), new List<Diagnostic>() { diagnostic });
        }
    }
}
