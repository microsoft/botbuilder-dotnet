// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the day of the year from a timestamp.
    /// </summary>
    public class DayOfYear : ExpressionEvaluator
    {
        public DayOfYear()
            : base(ExpressionType.DayOfYear, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.DayOfYear));
        }
    }
}
