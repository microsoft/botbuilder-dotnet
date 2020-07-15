// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of time units to a timestamp. 
    /// </summary>
    public class AddToTime : ExpressionEvaluator
    {
        public AddToTime()
            : base(ExpressionType.AddToTime, Evaluator, ReturnType.String, Validator)
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
                var format = (args.Count() == 4) ? (string)args[3] : FunctionUtils.DefaultDateTimeFormat;
                if (args[1].IsInteger() && args[2] is string timeUnit)
                {
                    (value, error) = EvalAddToTime(args[0], Convert.ToInt64(args[1]), timeUnit, format);
                }
                else
                {
                    error = $"{expression} should contain an ISO format timestamp, a time interval integer, a string unit of time and an optional output format string.";
                }
            }

            return (value, error);
        }

        private static (string, string) EvalAddToTime(object timestamp, long interval, string timeUnit, string format)
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
                    (result, error) = FunctionUtils.ReturnFormatTimeStampStr(addedTimeStamp, format);
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Object, ReturnType.Number, ReturnType.String);
        }
    }
}
