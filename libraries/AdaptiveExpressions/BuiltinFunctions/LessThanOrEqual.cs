// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether the first value is less than or equal to the second value.
    /// Return true if the first value is less than or equal, or return false if the first value is more.
    /// </summary>
    public class LessThanOrEqual : ComparisonEvaluator
    {
        public LessThanOrEqual()
            : base(
                  ExpressionType.LessThanOrEqual,
                  Function,
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.CultureInvariantDoubleConvert(args[0]) <= FunctionUtils.CultureInvariantDoubleConvert(args[1]);
        }
    }
}
