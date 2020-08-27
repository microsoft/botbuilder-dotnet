// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a number of ticks that the two timestamp differs.
    /// </summary>
    internal class DateTimeDiff : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeDiff"/> class.
        /// </summary>
        public DateTimeDiff()
            : base(ExpressionType.DateTimeDiff, Evaluator, ReturnType.Number, Validator)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object dateTimeStart = null;
            object dateTimeEnd = null;
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (dateTimeStart, error) = FunctionUtils.TicksWithError(args[0]);
                if (error == null)
                {
                    (dateTimeEnd, error) = FunctionUtils.TicksWithError(args[1]);
                }
                else
                {
                    error = $"{expression} must have two ISO timestamps.";
                }
            }

            if (error == null)
            {
                value = (long)dateTimeStart - (long)dateTimeEnd;
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 2, ReturnType.String);
        }
    }
}
