﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AdaptiveExpressions.Core.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace Microsoft.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return the next viable time of a timex expression based on the current time and user's timezone.
    /// </summary>
    internal class GetNextViableTime : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetNextViableTime"/> class.
        /// </summary>
        public GetNextViableTime()
            : base(ExpressionType.GetNextViableTime, Evaluator, ReturnType.String, FunctionUtils.ValidateUnaryOrBinaryString)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
                        TimexProperty parsed = null;
                        string result = null;
                        string error = null;
                        var (validHour, validMinute, validSecond) = (0, 0, 0);
                        var formatRegex = new Regex("TXX:[0-5][0-9]:[0-5][0-9]");
                        var currentUtcTime = DateTime.UtcNow;
                        var convertedDateTime = currentUtcTime;
                        IReadOnlyList<object> args;
                        (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
                        if (error == null)
                        {
                            if (!formatRegex.IsMatch(args[0] as string))
                            {
                                error = $"{args[0]}  must be a timex string which only contains minutes and seconds, for example: 'TXX:15:28'";
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
                            (parsed, error) = FunctionUtils.ParseTimexProperty((args[0] as string).Replace("XX", "00", StringComparison.Ordinal));
                        }

                        if (error == null)
                        {
                            var (hour, minute, second) = (convertedDateTime.Hour, convertedDateTime.Minute, convertedDateTime.Second);
                            if (parsed.Minute > minute || (parsed.Minute == minute && parsed.Second >= second))
                            {
                                validHour = hour;
                            }
                            else
                            {
                                validHour = hour + 1;
                            }

                            if (validHour >= 24)
                            {
                                validHour -= 24;
                            }

                            validMinute = parsed.Minute ?? 0;
                            validSecond = parsed.Second ?? 0;
                            result = TimexProperty.FromTime(new Time(validHour, validMinute, validSecond)).TimexValue;
                        }

                        return (result, error);
                    }
    }
}
