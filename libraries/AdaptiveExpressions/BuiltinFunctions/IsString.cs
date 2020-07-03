// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsString : ExpressionEvaluator
    {
        public IsString(string alias = null)
            : base(alias ?? ExpressionType.IsString, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => args[0] != null && args[0].GetType() == typeof(string));
        }
    }
}
