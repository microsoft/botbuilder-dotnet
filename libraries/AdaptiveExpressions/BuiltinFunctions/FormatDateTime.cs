// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a timestamp in the specified format.
    /// </summary>
    public class FormatDateTime : ExpressionEvaluator
    {
        public FormatDateTime()
            : base(ExpressionType.FormatDateTime, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var timestamp = args[0];
                            if (timestamp is string tsString)
                            {
                                (result, error) = ParseTimestamp(tsString, dt => dt.ToString(args.Count() == 2 ? args[1].ToString() : FunctionUtils.DefaultDateTimeFormat, CultureInfo.InvariantCulture));
                            }
                            else if (timestamp is DateTime dt)
                            {
                                result = dt.ToString(args.Count() == 2 ? args[1].ToString() : FunctionUtils.DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                error = $"formatDateTime has invalid first argument {timestamp}";
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
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Object);
        }
    }
}
