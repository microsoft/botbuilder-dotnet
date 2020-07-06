// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a random integer from a specified range, which is inclusive only at the starting end.
    /// </summary>
    public class Rand : ExpressionEvaluator
    {
        private static readonly Random Randomizer = new Random();

        private static readonly object _randomizerLock = new object();

        public Rand()
            : base(ExpressionType.Rand, Evaluator(), ReturnType.Number, FunctionUtils.ValidateBinaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            var min = Convert.ToInt32(args[0]);
                            var max = Convert.ToInt32(args[1]);
                            if (min >= max)
                            {
                                error = $"{min} is not < {max} for rand";
                            }
                            else
                            {
                                lock (_randomizerLock)
                                {
                                    value = Randomizer.Next(min, max);
                                }
                            }

                            return (value, error);
                        },
                        FunctionUtils.VerifyInteger);
        }
    }
}
