// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the month of the specified timestamp.
    /// </summary>
    public class Month : ExpressionEvaluator
    {
        public Month()
            : base(ExpressionType.Month, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.Month));
        }
    }
}
