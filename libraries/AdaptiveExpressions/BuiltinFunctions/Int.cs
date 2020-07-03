// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Int : ExpressionEvaluator
    {
        public Int(string alias = null)
            : base(alias ?? ExpressionType.Int, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Convert.ToInt64(args[0]));
        }
    }
}
