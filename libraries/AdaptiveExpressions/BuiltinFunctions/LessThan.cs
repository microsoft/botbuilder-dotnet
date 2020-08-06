// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.CultureInvariantDoubleConvert(args[0]) < FunctionUtils.CultureInvariantDoubleConvert(args[1]);
        }
    }
}
