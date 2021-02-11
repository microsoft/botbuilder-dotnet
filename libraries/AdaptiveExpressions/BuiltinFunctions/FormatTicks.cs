// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a timestamp in the specified format from ticks.
    /// FormatTicks function takes a ticks long integer,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class FormatTicks : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatTicks"/> class.
        /// </summary>
        public FormatTicks()
            : base(ExpressionType.FormatTicks, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithOptionsAndError(
                        (args, options) =>
                        {
                            object result = null;
                            string error = null;
                            var timestamp = args[0];
                            var format = FunctionUtils.DefaultDateTimeFormat;
                            var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
                            (format, locale, error) = FunctionUtils.DetermineFormatAndLocale(args, format, locale, 3);
                            if (error == null)
                            {
                                if (timestamp.IsInteger())
                                {
                                    var ticks = Convert.ToInt64(timestamp, CultureInfo.InvariantCulture);
                                    var dateTime = new DateTime(ticks);
                                    result = dateTime.ToString(format, locale);
                                }
                                else
                                {
                                    error = $"formatTicks first arugment {timestamp} must be an integer";
                                }
                            }

                            return (result, error);
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String, ReturnType.String }, ReturnType.Number);
        }
    }
}
