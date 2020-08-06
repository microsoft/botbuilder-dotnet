// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a timestamp in the specified format from UNIX time (also know as Epoch time, POSIX time, UNIX Epoch time).
    /// FormatEpoch function takes an epoch long integer,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class FormatEpoch : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatEpoch"/> class.
        /// </summary>
        public FormatEpoch()
            : base(ExpressionType.FormatEpoch, Evaluator(), ReturnType.String, Validator)
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
                                if (timestamp.IsNumber())
                                {
                                    var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                    dateTime = dateTime.AddSeconds(Convert.ToDouble(timestamp, CultureInfo.InvariantCulture));
                                    result = dateTime.ToString(format, locale);
                                }
                                else
                                {
                                    error = $"formatEpoch first argument {timestamp} is not a number";
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
