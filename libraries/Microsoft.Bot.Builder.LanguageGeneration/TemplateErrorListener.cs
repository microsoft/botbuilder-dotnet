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
    public class TemplateErrorListener : BaseErrorListener
    {
        private readonly string source;
        private readonly int startLine;

        public TemplateErrorListener(string errorSource, int startLine)
        {
            source = errorSource;
            this.startLine = startLine;
        }

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            var startPosition = new Position(startLine + line, charPositionInLine);
            var stopPosition = new Position(startLine + line, charPositionInLine + offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1);
            var range = new Range(startPosition, stopPosition);
            var diagnostic = new Diagnostic(range, "TemplateError", DiagnosticSeverity.Error, source);
            throw new TemplateException(diagnostic.ToString(), new List<Diagnostic>() { diagnostic });
        }
    }
}
