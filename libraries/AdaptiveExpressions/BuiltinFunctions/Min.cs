// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the lowest value from a set of numbers or an array.
    /// </summary>
    internal class Min : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Min"/> class.
        /// </summary>
        public Min()
            : base(ExpressionType.Min, Evaluator(), ReturnType.Number, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                args =>
            {
                object result = double.MaxValue;
                if (args.Count == 1)
                {
                    if (FunctionUtils.TryParseList(args[0], out IList ilist))
                    {
                        foreach (var value in ilist)
                        {
                            result = EvalMin(result, value);
                        }
                    }
                    else
                    {
                        result = EvalMin(result, args[0]);
                    }
                }
                else
                {
                    foreach (var arg in args)
                    {
                        if (FunctionUtils.TryParseList(arg, out IList ilist))
                        {
                            foreach (var value in ilist)
                            {
                                result = EvalMin(result, value);
                            }
                        }
                        else
                        {
                            result = EvalMin(result, arg);
                        }
                    }
                }

                return result;
            }, FunctionUtils.VerifyNumericListOrNumber);
        }

        private static object EvalMin(object a, object b)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (FunctionUtils.CultureInvariantDoubleConvert(a) <= FunctionUtils.CultureInvariantDoubleConvert(b))
            {
                return a;
            }

            return b;
        }
    }
}
