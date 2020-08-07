// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the day of the year from a timestamp.
    /// </summary>
    internal class DayOfYear : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfYear"/> class.
        /// </summary>
        public DayOfYear()
            : base(ExpressionType.DayOfYear, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => (dt.DayOfYear, null)));
        }
    }
}
