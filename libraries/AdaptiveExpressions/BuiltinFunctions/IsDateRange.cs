// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given TimexProperty or Timex expression refers to a valid date range.
    /// </summary>
    internal class IsDateRange : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsDateRange"/> class.
        /// </summary>
        public IsDateRange()
            : base(ExpressionType.IsDateRange, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            TimexProperty parsed = null;
            bool? value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (parsed, error) = FunctionUtils.ParseTimexProperty(args[0]);
            }

            if (error == null)
            {
                value = (parsed.Year != null && parsed.DayOfMonth == null) ||
                                    (parsed.Year != null && parsed.Month != null && parsed.DayOfMonth == null) ||
                                    (parsed.Month != null && parsed.DayOfMonth == null) ||
                                    parsed.Season != null || parsed.WeekOfYear != null || parsed.WeekOfMonth != null;
            }

            return (value, error);
        }
    }
}
