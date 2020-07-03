// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class DateTimeDiff : ExpressionEvaluator
    {
        public DateTimeDiff(string alias = null)
            : base(alias ?? ExpressionType.DateTimeDiff, EvalDateTimeDiff, ReturnType.Number, Validator)
        {
        }

        private static (object value, string error) EvalDateTimeDiff(Expression expression, IMemory state, Options options)
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
