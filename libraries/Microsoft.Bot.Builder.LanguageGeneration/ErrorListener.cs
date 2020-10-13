// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    /// <summary>
    /// LG parser error listener.
    /// </summary>
    internal class ErrorListener : BaseErrorListener
    {
        private readonly string _source;
        private readonly int _lineOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorListener"/> class.
        /// </summary>
        /// <param name="errorSource">String value that represents the source of the error.</param>
        /// <param name="lineOffset">Offset of the line where the error occurred.</param>
        public ErrorListener(string errorSource, int lineOffset = 0)
        {
            _source = errorSource;
            _lineOffset = lineOffset;
        }

        /// <inheritdoc/>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var startPosition = new Position(_lineOffset + line, charPositionInLine);
            var stopPosition = new Position(_lineOffset + line, charPositionInLine + offendingSymbol.StopIndex - offendingSymbol.StartIndex + 1);
            var range = new Range(startPosition, stopPosition);
            var diagnostic = new Diagnostic(range, TemplateErrors.SyntaxError(msg), DiagnosticSeverity.Error, _source);
            throw new TemplateException(diagnostic.ToString(), new List<Diagnostic>() { diagnostic });
        }
    }
}
