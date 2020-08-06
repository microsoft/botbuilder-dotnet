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
    /// Return the start of the day for a timestamp.
    /// StartOfDay function takes a timestamp string,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class StartOfDay : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartOfDay"/> class.
        /// </summary>
        public StartOfDay()
            : base(ExpressionType.StartOfDay, Evaluator, ReturnType.String, Validator)
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
                (format, locale, error) = FunctionUtils.DetermineFormatAndLocale(args, format, locale, 3);
            }

            if (error == null)
            {
                (value, error) = StartOfDayWithError(args[0], format, locale);
            }

            return (value, error);
        }

        private static (object, string) StartOfDayWithError(object timestamp, string format, CultureInfo locale)
        {
            string result = null;
            string error = null;
            object parsed = null;
            (parsed, error) = FunctionUtils.NormalizeToDateTime(timestamp);

            if (error == null)
            {
                var ts = (DateTime)parsed;
                var startOfDay = ts.Date;
                (result, error) = FunctionUtils.ReturnFormatTimeStampStr(startOfDay, format, locale);
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 3, ReturnType.String);
        }
    }
}
