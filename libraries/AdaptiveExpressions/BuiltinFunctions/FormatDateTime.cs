// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a timestamp in the specified format.
    /// FormatDateTime function takes a timestamp string,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class FormatDateTime : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatDateTime"/> class.
        /// </summary>
        public FormatDateTime()
            : base(ExpressionType.FormatDateTime, Evaluator(), ReturnType.String, Validator)
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
                                if (timestamp is string tsString)
                                {
                                    (result, error) = ParseTimestamp(tsString, dt => dt.ToString(format, locale));
                                }
                                else if (timestamp is DateTime dt)
                                {
                                    result = dt.ToString(format, locale);
                                }
                                else
                                {
                                    error = $"formatDateTime has invalid first argument {timestamp}";
                                }
                            }

                            return (result, error);
                        });
        }

        private static (object, string) ParseTimestamp(string timeStamp, Func<DateTime, object> transform = null)
        {
            object result = null;
            string error = null;
            if (DateTime.TryParse(
                    s: timeStamp,
                    provider: CultureInfo.InvariantCulture,
                    styles: DateTimeStyles.RoundtripKind,
                    result: out var parsed))
            {
                result = transform != null ? transform(parsed) : parsed;
            }
            else
            {
                error = $"Could not parse {timeStamp}";
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String, ReturnType.String }, ReturnType.Object);
        }
    }
}
