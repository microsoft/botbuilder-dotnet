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
    /// Convert a timestamp to Universal Time Coordinated (UTC) from the source time zone.
    /// ConvertToUtc function takes a timestamp string, a timezone string,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class ConvertToUtc : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertToUtc"/> class.
        /// </summary>
        public ConvertToUtc()
            : base(ExpressionType.ConvertToUtc, Evaluator, ReturnType.String, Validator)
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
                if (args[1] is string sourceTimeZone)
                {
                    (value, error) = EvalConvertToUTC(args[0], sourceTimeZone, format, locale);
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a origin time zone string, an optional output format string and an optional locale string.";
                }
            }

            return (value, error);
        }

        private static (string, string) EvalConvertToUTC(object sourceTimestamp, string sourceTimezone, string format, CultureInfo locale)
        {
            string error = null;
            string result = null;
            var srcDt = DateTime.UtcNow;
            try
            {
                if (sourceTimestamp is string st)
                {
                    srcDt = DateTime.Parse(st, CultureInfo.InvariantCulture);
                }
                else
                {
                    srcDt = (DateTime)sourceTimestamp;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types (we should probably do something about this but ignoring it for not)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = $"illegal time-stamp representation {sourceTimestamp}";
            }

            if (error == null)
            {
                object convertedTimeZone;
                (convertedTimeZone, error) = FunctionUtils.ConvertTimeZoneFormat(sourceTimezone);
                if (error == null)
                {
                    var convertedDateTime = TimeZoneInfo.ConvertTimeToUtc(srcDt, (TimeZoneInfo)convertedTimeZone);
                    (result, error) = FunctionUtils.ReturnFormatTimeStampStr(convertedDateTime, format, locale);
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 4, ReturnType.String);
        }
    }
}
