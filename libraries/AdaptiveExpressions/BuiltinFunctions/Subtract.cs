// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the result from subtracting the second number from the first number.
    /// </summary>
    internal class Subtract : MultivariateNumericEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subtract"/> class.
        /// </summary>
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
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a, CultureInfo.InvariantCulture) - Convert.ToInt64(b, CultureInfo.InvariantCulture);
            }

            return FunctionUtils.CultureInvariantDoubleConvert(a) - FunctionUtils.CultureInvariantDoubleConvert(b);
        }
    }
}
