// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Threading;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Format number into required decimal numbers.
    /// FormatNumber function takes a number as the first argument, a precision digits integer
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name..
    /// </summary>
    internal class FormatNumber : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatNumber"/> class.
        /// </summary>
        public FormatNumber()
            : base(ExpressionType.FormatNumber, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithOptionsAndError(
                        (args, options) =>
                        {
                            string result = null;
                            string error = null;
                            var locale = options.Locale != null ? new CultureInfo(options.Locale) : Thread.CurrentThread.CurrentCulture;
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

                            if (error == null)
                            {
                                (locale, error) = FunctionUtils.DetermineLocale(args, locale, 3);
                            }

                            if (error == null)
                            {
                                var precision = 0;
                                (precision, error) = FunctionUtils.ParseInt32(args[1]);
                                if (error == null)
                                {
                                    var number = Convert.ToDouble(args[0], CultureInfo.InvariantCulture);
                                    result = number.ToString("N" + precision.ToString(CultureInfo.InvariantCulture), locale);
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
