// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the integer result from dividing two numbers. 
    /// </summary>
    internal class Divide : MultivariateNumericEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Divide"/> class.
        /// </summary>
        public Divide()
            : base(ExpressionType.Divide, Evaluator, Verify)
        {
        }

        private static object Evaluator(IReadOnlyList<object> args)
        {
            return EvalDivide(args[0], args[1]);
        }

        private static string Verify(object val, Expression expression, int pos)
        {
            var error = FunctionUtils.VerifyNumber(val, expression, pos);
            if (error == null && (pos > 0 && Convert.ToSingle(val, CultureInfo.InvariantCulture) == 0.0))
            {
                error = $"Cannot divide by 0 from {expression}";
            }

            return error;
        }

        private static object EvalDivide(object a, object b)
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
                return Convert.ToInt64(a, CultureInfo.InvariantCulture) / Convert.ToInt64(b, CultureInfo.InvariantCulture);
            }

            return FunctionUtils.CultureInvariantDoubleConvert(a) / FunctionUtils.CultureInvariantDoubleConvert(b);
        }
    }
}
