// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the product from multiplying two numbers.
    /// </summary>
    public class Multiply : MultivariateNumericEvaluator
    {
        public Multiply()
            : base(ExpressionType.Multiply, Evaluator)
        {
        }

        private static object Evaluator(IReadOnlyList<object> args)
        {
            return EvalMultiply(args[0], args[1]);
        }

        private static object EvalMultiply(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a) * Convert.ToInt64(b);
            }
            else
            {
                return FunctionUtils.CultureInvariantDoubleConvert(a) * FunctionUtils.CultureInvariantDoubleConvert(b);
            }
        }
    }
}
