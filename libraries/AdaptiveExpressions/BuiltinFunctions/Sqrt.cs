// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Returns the square root of a specified number.
    /// </summary>
    internal class Sqrt : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sqrt"/> class.
        /// </summary>
        public Sqrt()
            : base(ExpressionType.Sqrt, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            object result = null;
                            var originalNumber = Convert.ToDouble(args[0], CultureInfo.InvariantCulture);
                            if (originalNumber < 0)
                            {
                                error = "Do not support square root extraction of negative numbers.";
                            }
                            else
                            {
                                result = Math.Sqrt(originalNumber);
                            }

                            return (result, error);
                        }, FunctionUtils.VerifyNumber);
        }
    }
}
