// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Convert a timestamp from Universal Time Coordinated (UTC) to a target time zone.
    /// </summary>
    public class ConvertFromUtc : ExpressionEvaluator
    {
        public ConvertFromUtc()
            : base(ExpressionType.ConvertFromUtc, Evaluator, ReturnType.String, Validator)
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
                var format = (args.Count() == 3) ? (string)args[2] : FunctionUtils.DefaultDateTimeFormat;
                if (args[1] is string targetTimeZone)
                {
                    (value, error) = EvalConvertFromUTC(args[0], targetTimeZone, format);
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a destination time zone string and an optional output format string.";
                }
            }

            return (value, error);
        }

        private static (string, string) EvalConvertFromUTC(object utcTimestamp, string timezone, string format)
        {
            string error = null;
            string result = null;
            var utcDt = DateTime.UtcNow;
            object parsed = null;
            object convertedTimeZone = null;
            (parsed, error) = FunctionUtils.NormalizeToDateTime(utcTimestamp);
            if (error == null)
            {
                utcDt = ((DateTime)parsed).ToUniversalTime();
            }

            if (error == null)
            {
                (convertedTimeZone, error) = FunctionUtils.ConvertTimeZoneFormat(timezone);

                if (error == null)
                {
                    var convertedDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDt, (TimeZoneInfo)convertedTimeZone);
                    (result, error) = FunctionUtils.ReturnFormatTimeStampStr(convertedDateTime, format);
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 3, ReturnType.String);
        }
    }
}
