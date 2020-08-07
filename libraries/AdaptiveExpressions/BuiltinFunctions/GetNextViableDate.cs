// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the next viable date of a timex expression based on the current date and user's timezone.
    /// </summary>
    internal class GetNextViableDate : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetNextViableDate"/> class.
        /// </summary>
        public GetNextViableDate()
            : base(ExpressionType.GetNextViableDate, Evaluator, ReturnType.String, FunctionUtils.ValidateUnaryOrBinaryString)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            TimexProperty parsed = null;
            string result = null;
            string error = null;
            var (validYear, validMonth, validDay) = (0, 0, 0);
            var currentUtcTime = DateTime.UtcNow;
            var convertedDateTime = currentUtcTime;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (parsed, error) = FunctionUtils.ParseTimexProperty(args[0]);
            }

            if (error == null)
            {
                if (parsed.Year != null || parsed.Month == null || parsed.DayOfMonth == null)
                {
                    error = $"{args[0]} must be a timex string which only contains month and day-of-month, for example: 'XXXX-10-31'.";
                }
            }

            if (error == null)
            {
                if (args.Count == 2 && args[1] is string timezone)
                {
                    object convertedTimeZone = null;
                    (convertedTimeZone, error) = FunctionUtils.ConvertTimeZoneFormat(timezone);
                    if (error == null)
                    {
                        convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(currentUtcTime, (TimeZoneInfo)convertedTimeZone);
                    }
                }
                else
                {
                    convertedDateTime = currentUtcTime.ToLocalTime();
                }
            }

            if (error == null)
            {
                var (year, month, day) = (convertedDateTime.Year, convertedDateTime.Month, convertedDateTime.Day);
                if (parsed.Month > month || (parsed.Month == month && parsed.DayOfMonth >= day))
                {
                    validYear = year;
                }
                else
                {
                    validYear = year + 1;
                }

                validMonth = parsed.Month ?? 0;
                validDay = parsed.DayOfMonth ?? 0;

                if (validMonth == 2 && validDay == 29)
                {
                    while (!DateTime.IsLeapYear(validYear))
                    {
                        validYear += 1;
                    }
                }

                result = TimexProperty.FromDate(new DateTime(validYear, validMonth, validDay)).TimexValue;
            }

            return (result, error);
        }
    }
}
