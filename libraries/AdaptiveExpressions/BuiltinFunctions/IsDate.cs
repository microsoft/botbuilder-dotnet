// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given TimexProperty or Timex expression refers to a valid date.
    /// Valid dates contain the month and dayOfMonth, or contain the dayOfWeek.
    /// </summary>
    internal class IsDate : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsDate"/> class.
        /// </summary>
        public IsDate()
            : base(ExpressionType.IsDate, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnary)
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
                value = (parsed.Month != null && parsed.DayOfMonth != null) || parsed.DayOfWeek != null;
            }

            return (value, error);
        }
    }
}
