// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class UriComponentToString : ExpressionEvaluator
    {
        public UriComponentToString()
            : base(ExpressionType.UriComponentToString, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Uri.UnescapeDataString(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
