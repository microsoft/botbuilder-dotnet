// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the result from subtracting the second number from the first number.
    /// </summary>
    public class Subtract : MultivariateNumericEvaluator
    {
        public Subtract()
            : base(ExpressionType.Subtract, Evaluator)
        {
        }

        private static object Evaluator(IReadOnlyList<object> args)
        {
            return EvalSubtract(args[0], args[1]);
        }

        private static object EvalSubtract(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a) - Convert.ToInt64(b);
            }
            else
            {
                return FunctionUtils.CultureInvariantDoubleConvert(a) - FunctionUtils.CultureInvariantDoubleConvert(b);
            }
        }
    }
}
