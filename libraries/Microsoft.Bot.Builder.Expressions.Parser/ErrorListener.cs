// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Data;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace Microsoft.Bot.Builder.Expressions
{
    public class ErrorListener : BaseErrorListener
    {
        public static readonly ErrorListener Instance = new ErrorListener();

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e)
        {
            throw new SyntaxErrorException(msg) { Source = $"({line}:{charPositionInLine})", };
        }
    }
}
