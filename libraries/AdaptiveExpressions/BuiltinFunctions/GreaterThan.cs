// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether the first value is greater than the second value.
    /// Return true if the first value is more, or return false if less.
    /// </summary>
    internal class GreaterThan : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GreaterThan"/> class.
        /// </summary>
        public GreaterThan()
            : base(
                  ExpressionType.GreaterThan,
                  Function,
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.CultureInvariantDoubleConvert(args[0]) > FunctionUtils.CultureInvariantDoubleConvert(args[1]);
        }
    }
}
