// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG parser error listener.
    /// </summary>
    public class ErrorListener : BaseErrorListener
    {
        private readonly string source;
        private readonly int lineOffset;

        public ErrorListener(string errorSource, int lineOffset = 0)
        {
            source = errorSource;
            this.lineOffset = lineOffset;
        }

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            var startPosition = new Position(lineOffset + line, charPositionInLine);
            var stopPosition = new Position(lineOffset + line, charPositionInLine + offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1);
            var range = new Range(startPosition, stopPosition);
            var diagnostic = new Diagnostic(range, TemplateErrors.SyntaxError, DiagnosticSeverity.Error, source);
            throw new TemplateException(diagnostic.ToString(), new List<Diagnostic>() { diagnostic });
        }
    }
}
