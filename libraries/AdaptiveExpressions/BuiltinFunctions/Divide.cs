// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the integer result from dividing two numbers. 
    /// </summary>
    public class Divide : MultivariateNumericEvaluator
    {
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
            if (error == null && (pos > 0 && Convert.ToSingle(val) == 0.0))
            {
                error = $"Cannot divide by 0 from {expression}";
            }

            return error;
        }

        private static object EvalDivide(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a) / Convert.ToInt64(b);
            }
            else
            {
                return FunctionUtils.CultureInvariantDoubleConvert(a) / FunctionUtils.CultureInvariantDoubleConvert(b);
            }
        }
    }
}
