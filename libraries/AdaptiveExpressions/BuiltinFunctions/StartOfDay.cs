﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class StartOfDay : ExpressionEvaluator
    {
        public StartOfDay(string alias = null)
            : base(alias ?? ExpressionType.StartOfDay, EvalStartOfDay, ReturnType.String, Validator)
        {
        }

        private static (object value, string error) EvalStartOfDay(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                var format = (args.Count() == 2) ? (string)args[1] : FunctionUtils.DefaultDateTimeFormat;
                (value, error) = StartOfDayWithError(args[0], format);
            }

            return (value, error);
        }

        private static (object, string) StartOfDayWithError(object timestamp, string format)
        {
            string result = null;
            string error = null;
            object parsed = null;
            (parsed, error) = FunctionUtils.NormalizeToDateTime(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                var startOfDay = ts.Date;
                (result, error) = FunctionUtils.ReturnFormatTimeStampStr(startOfDay, format);
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 2, ReturnType.String);
        }
    }
}
