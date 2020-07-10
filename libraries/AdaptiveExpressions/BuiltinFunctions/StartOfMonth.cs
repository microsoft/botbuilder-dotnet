// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the start of the month for a timestamp.
    /// </summary>
    public class StartOfMonth : ExpressionEvaluator
    {
        public StartOfMonth()
            : base(ExpressionType.StartOfMonth, Evaluator, ReturnType.String, Validator)
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
                (value, error) = StartOfMonthWithError(args[0], format);
            }

            return (value, error);
        }

        private static (object, string) StartOfMonthWithError(object timestamp, string format)
        {
            string result = null;
            object parsed = null;
            string error = null;
            (parsed, error) = FunctionUtils.NormalizeToDateTime(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                var startOfDay = ts.Date;
                var days = ts.Day;
                var startOfMonth = startOfDay.AddDays(1 - days);
                (result, error) = FunctionUtils.ReturnFormatTimeStampStr(startOfMonth, format);
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 2, ReturnType.String);
        }
    }
}
