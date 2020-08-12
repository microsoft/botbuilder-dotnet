// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a random integer from a specified range, which is inclusive only at the starting end.
    /// </summary>
    internal class Rand : ExpressionEvaluator
    {
        private static readonly object _randomizerLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="Rand"/> class.
        /// </summary>
        public Rand()
            : base(ExpressionType.Rand, Evaluator(), ReturnType.Number, FunctionUtils.ValidateBinaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithOptionsAndError(
                        (args, options) =>
                        {
                            object value = null;
                            string error = null;
                            var min = 0;
                            var max = 0;
                            (min, error) = FunctionUtils.ParseInt32(args[0]);
                            if (error == null)
                            {
                                (max, error) = FunctionUtils.ParseInt32(args[1]);
                            }

                            if (min >= max)
                            {
                                error = $"{min} is not < {max} for rand";
                            }
                            else
                            {
                                if (options.RandomValue != null)
                                {
                                    value = options.RandomValue;
                                }
                                else
                                {
                                    var random = options.RandomSeed == null ? new Random() : new Random(options.RandomSeed.Value);
 
                                    lock (_randomizerLock)
                                    {
                                        value = random.Next(min, max);
                                    }
                                }
                            }

                            return (value, error);
                        },
                        FunctionUtils.VerifyInteger);
        }
    }
}
