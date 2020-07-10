// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is a Boolean.
    /// </summary>
    public class IsBoolean : ExpressionEvaluator
    {
        public IsBoolean()
            : base(ExpressionType.IsBoolean, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => args[0] is bool);
        }
    }
}
