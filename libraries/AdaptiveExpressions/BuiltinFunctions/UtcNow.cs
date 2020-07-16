// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the current timestamp.
    /// </summary>
    public class UtcNow : ExpressionEvaluator
    {
        public UtcNow()
            : base(ExpressionType.UtcNow, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithOptionsAndError((args, options) =>
            {
                string error = null;
                string format = FunctionUtils.DefaultDateTimeFormat;
                object result = null;
                var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
                (format, locale, error) = FunctionUtils.DetermineFormatAndLocale(args, format, locale, 2);
                result = DateTime.UtcNow.ToString(format, locale);

                return (result, error);
            });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String, ReturnType.String });
        }
    }
}
