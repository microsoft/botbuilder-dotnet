// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given TimexProperty or Timex expression refers to a valid duration.
    /// </summary>
    internal class IsDuration : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsDuration"/> class.
        /// </summary>
        public IsDuration()
            : base(ExpressionType.IsDuration, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnary)
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
                value = parsed.Years != null || parsed.Months != null || parsed.Weeks != null || parsed.Days != null ||
                   parsed.Hours != null || parsed.Minutes != null || parsed.Seconds != null;
            }

            return (value, error);
        }
    }
}
