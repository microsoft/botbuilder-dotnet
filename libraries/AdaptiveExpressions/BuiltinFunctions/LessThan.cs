// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether the first value is less than the second value.
    /// Return true if the first value is less, or return false if the first value is more.
    /// </summary>
    internal class LessThan : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LessThan"/> class.
        /// </summary>
        public LessThan()
            : base(
                  ExpressionType.LessThan,
                  Function,
                  FunctionUtils.ValidateBinary,
                  FunctionUtils.VerifyNotNull)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            if (args[0].IsNumber() && args[1].IsNumber())
            {
                return FunctionUtils.CultureInvariantDoubleConvert(args[0]) < FunctionUtils.CultureInvariantDoubleConvert(args[1]);
            }

            if (args[0] is IComparable left && args[1] is IComparable right)
            {
                if (left.GetType() == right.GetType())
                {
                    return left.CompareTo(right) < 0;
                }
                else
                {
                    throw new ArgumentException($"{args[0]} and {args[1]} must have the same type.");
                }
            }
            else
            {
                throw new ArgumentException($"Both {args[0]} and {args[1]} must be comparable.");
            }
        }
    }
}
