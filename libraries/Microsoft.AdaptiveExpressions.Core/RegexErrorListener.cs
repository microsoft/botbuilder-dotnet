﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Antlr4.Runtime;

namespace Microsoft.AdaptiveExpressions.Core
{
    internal class RegexErrorListener : BaseErrorListener
    {
        public static readonly RegexErrorListener Instance = new RegexErrorListener();

        public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
        {
            throw new InvalidOperationException($"Regular expression is invalid.");
        }
    }
}
