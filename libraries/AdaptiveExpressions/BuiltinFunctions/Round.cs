// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Rounds a number value to the nearest integer.
    /// </summary>
    public class Round : ExpressionEvaluator
    {
        public Round()
            : base(ExpressionType.Round, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnaryOrBinaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            object result = null;
                            if (args.Count == 2 && !args[1].IsInteger())
                            {
                                error = $"The second parameter {args[1]} must be an integer.";
                            }

                            if (error == null)
                            {
                                var digits = args.Count == 2 ? Convert.ToInt32(args[1]) : 0;
                                if (digits < 0 || digits > 15)
                                {
                                    error = $"The second parameter {args[1]} must be an integer between 0 and 15.";
                                }
                                else
                                {
                                    result = Math.Round(Convert.ToDouble(args[0]), digits);
                                }
                            }

                            return (result, error);
                        }, FunctionUtils.VerifyNumber);
        }
    }
}
