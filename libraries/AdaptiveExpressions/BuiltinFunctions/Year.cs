// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the year of the specified timestamp.
    /// </summary>
    internal class Year : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Year"/> class.
        /// </summary>
        public Year()
            : base(ExpressionType.Year, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => (dt.Year, null)));
        }
    }
}
