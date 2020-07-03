// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the current timestamp minus the specified time units.
    /// </summary>
    public class GetPastTime : ExpressionEvaluator
    {
        public GetPastTime()
            : base(ExpressionType.GetPastTime, Evaluator, ReturnType.String, Validator)
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
                if (args[0].IsInteger() && args[1] is string string1)
                {
                    var format = (args.Count() == 3) ? (string)args[2] : FunctionUtils.DefaultDateTimeFormat;
                    Func<DateTime, DateTime> timeConverter;
                    (timeConverter, error) = FunctionUtils.DateTimeConverter(Convert.ToInt64(args[0]), string1);
                    if (error == null)
                    {
                        value = timeConverter(DateTime.UtcNow).ToString(format);
                    }
                }
                else
                {
                    error = $"{expression} should contain a time interval integer, a string unit of time and an optional output format string.";
                }
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Number, ReturnType.String);
        }
    }
}
