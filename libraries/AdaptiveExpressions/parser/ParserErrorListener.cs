// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Data;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Expression parser error listener.
    /// </summary>
    public class ParserErrorListener : BaseErrorListener
    {
        public static readonly ParserErrorListener Instance = new ParserErrorListener();

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new SyntaxErrorException(msg) { Source = $"({line}:{charPositionInLine})", };
        }
    }
}
