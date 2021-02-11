// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the day of the week from a timestamp.
    /// </summary>
    internal class DayOfWeek : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DayOfWeek"/> class.
        /// </summary>
        public DayOfWeek()
            : base(ExpressionType.DayOfWeek, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => FunctionUtils.ParseInt32(dt.DayOfWeek)));
        }
    }
}
