// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the start of the hour for a timestamp.
    /// </summary>
    public class StartOfHour : ExpressionEvaluator
    {
        public StartOfHour()
            : base(ExpressionType.StartOfHour, Evaluator, ReturnType.String, Validator)
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
                var format = (args.Count() == 2) ? (string)args[1] : FunctionUtils.DefaultDateTimeFormat;
                (value, error) = StartOfHourWithError(args[0], format);
            }

            return (value, error);
        }

        private static (object, string) StartOfHourWithError(object timestamp, string format)
        {
            string result = null;
            string error = null;
            object parsed = null;
            (parsed, error) = FunctionUtils.NormalizeToDateTime(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                var startOfDay = ts.Date;
                var hours = ts.Hour;
                var startOfHour = startOfDay.AddHours(hours);
                (result, error) = FunctionUtils.ReturnFormatTimeStampStr(startOfHour, format);
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 2, ReturnType.String);
        }
    }
}
