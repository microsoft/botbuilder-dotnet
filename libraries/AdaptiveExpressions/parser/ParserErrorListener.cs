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
        /// <summary>
        /// A ParserErrorListener instance.
        /// </summary>
        public static readonly ParserErrorListener Instance = new ParserErrorListener();

        /// <summary>
        /// Throw a syntax error based one current context.
        /// </summary>
        /// <param name="recognizer">An Antlr4 runtime recognizer.</param>
        /// <param name="offendingSymbol">The token violate the lexer rules.</param>
        /// <param name="line">The line number where the error occurred.</param>
        /// <param name="charPositionInLine">The position of character in the line where the error occurred.</param>
        /// <param name="msg">The error message.</param>
        /// <param name="e">The RecognitionException.</param>
        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new SyntaxErrorException(msg) { Source = $"({line}:{charPositionInLine})", };
        }
    }
}
