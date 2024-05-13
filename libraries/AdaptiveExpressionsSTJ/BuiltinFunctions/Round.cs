// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Rounds a number value to the nearest integer.
    /// </summary>
    internal class Round : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Round"/> class.
        /// </summary>
        public Round()
            : base(ExpressionType.Round, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnaryOrBinaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            object result = null;
                            if (args.Count == 2 && !args[1].IsInteger())
                            {
                                error = $"The second {args[1]} parameter must be an integer.";
                            }

                            if (error == null)
                            {
                                var digits = 0;
                                if (args.Count == 2)
                                {
                                    (digits, error) = FunctionUtils.ParseInt32(args[1]);
                                }

                                if (error == null)
                                {
                                    if (digits < 0 || digits > 15)
                                    {
                                        error = $"The second parameter {args[1]} must be an integer between 0 and 15;";
                                    }
                                    else
                                    {
                                        result = Math.Round(Convert.ToDouble(args[0], CultureInfo.InvariantCulture), digits);
                                    }
                                }
                            }

                            return (result, error);
                        }, FunctionUtils.VerifyNumber);
        }
    }
}
