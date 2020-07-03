// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class UriComponent : ExpressionEvaluator
    {
        public UriComponent()
            : base(ExpressionType.UriComponent, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Uri.EscapeDataString(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
