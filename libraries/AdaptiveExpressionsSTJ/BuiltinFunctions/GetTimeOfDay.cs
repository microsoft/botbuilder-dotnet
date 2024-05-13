// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Returns time of day for a given timestamp.
    /// </summary>
    internal class GetTimeOfDay : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetTimeOfDay"/> class.
        /// </summary>
        public GetTimeOfDay()
            : base(ExpressionType.GetTimeOfDay, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            (value, error) = FunctionUtils.NormalizeToDateTime(args[0]);
                            var timestamp = DateTime.Now;
                            if (error == null)
                            {
                                timestamp = (DateTime)value;
                            }
                            else if (DateTime.TryParseExact(
                                   s: args[0].ToString(),
                                   format: ConvertFromUtc.DefaultFormat,
                                   provider: CultureInfo.InvariantCulture,
                                   style: DateTimeStyles.RoundtripKind,
                                   result: out var parsed))
                            {
                                error = null;
                                timestamp = parsed;
                            }

                            if (error == null)
                            {
                                if (timestamp.Hour == 0 && timestamp.Minute == 0)
                                {
                                    value = "midnight";
                                }
                                else if (timestamp.Hour >= 0 && timestamp.Hour < 12)
                                {
                                    value = "morning";
                                }
                                else if (timestamp.Hour == 12 && timestamp.Minute == 0)
                                {
                                    value = "noon";
                                }
                                else if (timestamp.Hour < 18)
                                {
                                    value = "afternoon";
                                }
                                else if (timestamp.Hour < 22 || (timestamp.Hour == 22 && timestamp.Minute == 0))
                                {
                                    value = "evening";
                                }
                                else
                                {
                                    value = "night";
                                }
                            }

                            return (value, error);
                        });
        }
    }
}
