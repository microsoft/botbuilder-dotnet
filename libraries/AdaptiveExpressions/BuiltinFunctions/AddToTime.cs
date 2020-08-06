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
    /// Add a number of time units to a timestamp. 
    /// AddToTime function takes a timestamp string, an interval integer, a unit of time string,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class AddToTime : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddToTime"/> class.
        /// </summary>
        public AddToTime()
            : base(ExpressionType.AddToTime, Evaluator, ReturnType.String, Validator)
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
                if (args[1].IsInteger() && args[2] is string timeUnit)
                {
                    (value, error) = EvalAddToTime(args[0], Convert.ToInt64(args[1], CultureInfo.InvariantCulture), timeUnit, format, locale);
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a time interval integer, a string unit of time, an optional output format string and an optional locale string.";
                }
            }

            return (value, error);
        }

        private static (string, string) EvalAddToTime(object timestamp, long interval, string timeUnit, string format, CultureInfo locale)
        {
            string result = null;
            string error = null;
            object parsed = null;
            (parsed, error) = FunctionUtils.NormalizeToDateTime(timestamp);
            if (error == null)
            {
                var ts = (DateTime)parsed;
                Func<DateTime, DateTime> converter;
                (converter, error) = FunctionUtils.DateTimeConverter(interval, timeUnit, false);
                if (error == null)
                {
                    var addedTimeStamp = converter(ts);
                    (result, error) = FunctionUtils.ReturnFormatTimeStampStr(addedTimeStamp, format, locale);
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String, ReturnType.String }, ReturnType.Object, ReturnType.Number, ReturnType.String);
        }
    }
}
