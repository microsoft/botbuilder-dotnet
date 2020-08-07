// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the day of the month from a timestamp.
    /// </summary>
    internal class DayOfMonth : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfMonth"/> class.
        /// </summary>
        public DayOfMonth()
            : base(ExpressionType.DayOfMonth, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => (dt.Day, null)));
        }
    }
}
