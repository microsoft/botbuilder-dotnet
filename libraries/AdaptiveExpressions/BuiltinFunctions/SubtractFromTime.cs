// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
            var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
            var format = FunctionUtils.DefaultDateTimeFormat;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);

            if (error == null)
            {
                (format, locale, error) = FunctionUtils.DetermineFormatAndLocale(args, format, locale, 5);
            }

            if (error == null)
            {
                if (args[1].IsInteger() && args[2] is string string2)
                {
                    Func<DateTime, DateTime> timeConverter;
                    (timeConverter, error) = FunctionUtils.DateTimeConverter(Convert.ToInt64(args[1]), string2);
                    if (error == null)
                    {
                        (value, error) = FunctionUtils.NormalizeToDateTime(args[0], dt => timeConverter(dt).ToString(format, locale));
                    }
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a time interval integer, a string unit of time, an optional output format string and an optional locale string.";
                }
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String, ReturnType.String }, ReturnType.Object, ReturnType.Number, ReturnType.String);
        }
    }
}
