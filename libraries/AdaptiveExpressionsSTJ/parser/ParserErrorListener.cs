// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Data;
using System.IO;
using Antlr4.Runtime;

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
        /// <param name="output">Text writer.</param>
        /// <param name="recognizer">An Antlr4 runtime recognizer.</param>
        /// <param name="offendingSymbol">The token violate the lexer rules.</param>
        /// <param name="line">The line number where the error occurred.</param>
        /// <param name="charPositionInLine">The position of character in the line where the error occurred.</param>
        /// <param name="msg">The error message.</param>
        /// <param name="e">The RecognitionException.</param>
        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var syntaxErrorMessage = "Invalid expression format.";
            throw new SyntaxErrorException(syntaxErrorMessage) { Source = $"({line}:{charPositionInLine})", };
        }
    }
}
