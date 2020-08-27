// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the remainder from dividing two numbers. 
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
    internal class Mod : ExpressionEvaluator
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mod"/> class.
        /// </summary>
        public Mod()
            : base(ExpressionType.Mod, Evaluator(), ReturnType.Number, FunctionUtils.ValidateBinaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error;
                            if (Convert.ToInt64(args[1], CultureInfo.InvariantCulture) == 0)
                            {
                                error = $"Cannot mod by 0";
                            }
                            else
                            {
                                error = null;
                                value = EvalMod(args[0], args[1]);
                            }

                            return (value, error);
                        },
                        FunctionUtils.VerifyInteger);
        }

        private static object EvalMod(object a, object b)
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
                return Convert.ToInt64(a, CultureInfo.InvariantCulture) % Convert.ToInt64(b, CultureInfo.InvariantCulture);
            }

            return FunctionUtils.CultureInvariantDoubleConvert(a) % FunctionUtils.CultureInvariantDoubleConvert(b);
        }
    }
}
