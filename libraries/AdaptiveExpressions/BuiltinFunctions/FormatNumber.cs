// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Format number into required decimal numbers.
    /// </summary>
    public class FormatNumber : ExpressionEvaluator
    {
        public FormatNumber()
            : base(ExpressionType.FormatNumber, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string result = null;
                            string error = null;
                            if (!args[0].IsNumber())
                            {
                                error = $"formatNumber first argument {args[0]} must be number";
                            }
                            else if (!args[1].IsInteger())
                            {
                                error = $"formatNumber second argument {args[1]} must be number";
                            }
                            else if (args.Count == 3 && !(args[2] is string))
                            {
                                error = $"formatNumber third agument {args[2]} must be a locale";
                            }
                            else
                            {
                                var precision = 0;
                                (precision, error) = FunctionUtils.ParseInt32(args[1]);
                                try
                                {
                                    var number = Convert.ToDouble(args[0], CultureInfo.InvariantCulture);
                                    var locale = args.Count == 3 ? new CultureInfo(args[2] as string) : CultureInfo.InvariantCulture;
                                    result = number.ToString("N" + precision, locale);
                                }
#pragma warning disable CA1031 // Do not catch general exception types (we are capturing the exception and returning it)
                                catch
#pragma warning restore CA1031 // Do not catch general exception types
                                {
                                    error = $"{args[3]} is not a valid locale for formatNumber";
                                }
                            }

                            return (result, error);
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Number, ReturnType.Number);
        }
    }
}
