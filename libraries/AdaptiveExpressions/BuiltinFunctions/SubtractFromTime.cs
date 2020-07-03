// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Subtract a number of time units from a timestamp.
    /// </summary>
    public class SubtractFromTime : ExpressionEvaluator
    {
        public SubtractFromTime()
            : base(ExpressionType.SubtractFromTime, Evaluator, ReturnType.String, Validator)
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
                if (args[1].IsInteger() && args[2] is string string2)
                {
                    var format = (args.Count() == 4) ? (string)args[3] : FunctionUtils.DefaultDateTimeFormat;
                    Func<DateTime, DateTime> timeConverter;
                    (timeConverter, error) = FunctionUtils.DateTimeConverter(Convert.ToInt64(args[1]), string2);
                    if (error == null)
                    {
                        (value, error) = FunctionUtils.NormalizeToDateTime(args[0], dt => timeConverter(dt).ToString(format));
                    }
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a time interval integer, a string unit of time and an optional output format string.";
                }
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Object, ReturnType.Number, ReturnType.String);
        }
    }
}
