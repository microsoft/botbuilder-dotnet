// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.Expressions
{
    public class ExpressionErrorListener : BaseErrorListener
    {
        public static readonly ExpressionErrorListener Instance = new ExpressionErrorListener();

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new Exception($"line {line}:{charPositionInLine} {msg}");
        }
    }
}
