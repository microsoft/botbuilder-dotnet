// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a timestamp in the specified format from UNIX time (also know as Epoch time, POSIX time, UNIX Epoch time).
    /// </summary>
    public class FormatEpoch : ExpressionEvaluator
    {
        public FormatEpoch()
            : base(ExpressionType.FormatEpoch, Evaluator(), ReturnType.String, Validator)
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
                            if (timestamp.IsNumber())
                            {
                                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                                dateTime = dateTime.AddSeconds(Convert.ToDouble(timestamp));
                                result = dateTime.ToString(args.Count() == 2 ? args[1].ToString() : FunctionUtils.DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                error = $"formatEpoch first argument {timestamp} is not a number";
                            }

                            return (result, error);
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Number);
        }
    }
}
