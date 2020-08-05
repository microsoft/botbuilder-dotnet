// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Convert ticks to hours number.
    /// </summary>
    internal class TicksToHours : ExpressionEvaluator
    {
        private const long TicksPerHour = 60 * 60 * 10000000L;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicksToHours"/> class.
        /// </summary>
        public TicksToHours()
            : base(ExpressionType.TicksToHours, Evaluator, ReturnType.Number, FunctionUtils.ValidateUnaryNumber)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0].IsInteger())
                {
                    value = Convert.ToDouble(args[0], CultureInfo.InvariantCulture) / TicksPerHour;
                }
                else
                {
                    error = $"{expression} should contain an integer of ticks";
                }
            }

            return (value, error);
        }
    }
}
