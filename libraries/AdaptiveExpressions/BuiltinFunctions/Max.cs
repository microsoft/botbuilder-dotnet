// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the highest value from a list or array. The list or arary is inclusive at both ends.
    /// </summary>
    public class Max : ExpressionEvaluator
    {
        public Max()
            : base(ExpressionType.Max, Evaluator(), ReturnType.Number, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                args =>
                {
                    object result = double.MinValue;
                    if (args.Count == 1)
                    {
                        if (FunctionUtils.TryParseList(args[0], out IList ilist))
                        {
                            foreach (var value in ilist)
                            {
                                result = EvalMax(result, value);
                            }
                        }
                        else
                        {
                            result = EvalMax(result, args[0]);
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
                                    result = EvalMax(result, value);
                                }
                            }
                            else
                            {
                                result = EvalMax(result, arg);
                            }
                        }
                    }

                    return result;
                }, FunctionUtils.VerifyNumericListOrNumber);
        }

        private static object EvalMax(object a, object b)
        {
            if (a == null || b == null)
            {
                throw new ArgumentNullException();
            }

            if (FunctionUtils.CultureInvariantDoubleConvert(a) > FunctionUtils.CultureInvariantDoubleConvert(b))
            {
                return a;
            }
            else
            {
                return b;
            }
        }
    }
}
