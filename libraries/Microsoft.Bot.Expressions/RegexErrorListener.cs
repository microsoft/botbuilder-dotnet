// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.RegularExpressions;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace Microsoft.Bot.Expressions
{
    internal class RegexErrorListener : BaseErrorListener
    {
        public static readonly RegexErrorListener Instance = new RegexErrorListener();

        public override void SyntaxError([NotNull] IRecognizer recognizer, [Nullable] IToken offendingSymbol, int line, int charPositionInLine, [NotNull] string msg, [Nullable] RecognitionException e) => throw new Exception($"Regular expression is invalid.");
    }
}
