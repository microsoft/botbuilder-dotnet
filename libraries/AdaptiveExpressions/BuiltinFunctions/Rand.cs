// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

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
                                if (options.Properties.TryGetValue("randomValue", out var randomValue)
                                && randomValue.IsInteger())
                                {
                                    var randomValueNum = Convert.ToInt32(randomValue, CultureInfo.InvariantCulture);
                                    value = min + (randomValueNum % (max - min));
                                }
                                else
                                {
                                    var random = new Random();
                                    if (options.Properties.TryGetValue("randomSeed", out var randomSeed))
                                    {
                                        if (randomSeed.IsInteger())
                                        {
                                            var seed = Convert.ToInt32(randomValue, CultureInfo.InvariantCulture);
                                            random = new Random(seed);
                                        }
                                    }

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
