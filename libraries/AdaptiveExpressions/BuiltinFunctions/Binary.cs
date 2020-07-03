// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Binary : ExpressionEvaluator
    {
        public Binary(string alias = null)
            : base(alias ?? ExpressionType.Binary, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.ToBinary(args[0].ToString()));
        }
    }
}
