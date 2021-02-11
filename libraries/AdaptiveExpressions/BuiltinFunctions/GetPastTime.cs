// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the current timestamp minus the specified time units.
    /// GetPastTime function takes an interval integer, a unit of time string,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class GetPastTime : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetPastTime"/> class.
        /// </summary>
        public GetPastTime()
            : base(ExpressionType.GetPastTime, Evaluator, ReturnType.String, Validator)
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
                (format, locale, error) = FunctionUtils.DetermineFormatAndLocale(args, format, locale, 4);
            }

            if (error == null)
            {
                if (args[0].IsInteger() && args[1] is string string1)
                {
                    Func<DateTime, DateTime> timeConverter;
                    (timeConverter, error) = FunctionUtils.DateTimeConverter(Convert.ToInt64(args[0], CultureInfo.InvariantCulture), string1);
                    if (error == null)
                    {
                        value = timeConverter(DateTime.UtcNow).ToString(format, locale);
                    }
                }
                else
                {
                    error = $"{expression} should contain a time interval integer, a string unit of time, an optional output format string and an optional locale string.";
                }
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String, ReturnType.String }, ReturnType.Number, ReturnType.String);
        }
    }
}
