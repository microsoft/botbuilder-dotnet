// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the remainder from dividing two numbers. 
    /// </summary>
    public class Mod : ExpressionEvaluator
    {
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
                            if (Convert.ToInt64(args[1]) == 0)
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
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (a.IsInteger() && b.IsInteger())
            {
                return Convert.ToInt64(a) % Convert.ToInt64(b);
            }
            else
            {
                return FunctionUtils.CultureInvariantDoubleConvert(a) % FunctionUtils.CultureInvariantDoubleConvert(b);
            }
        }
    }
}
