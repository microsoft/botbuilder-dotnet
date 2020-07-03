// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether the first value is greater than or equal to the second value. Return true when the first value is greater or equal,
    /// or return false if the first value is less.
    /// </summary>
    public class GreaterThanOrEqual : ComparisonEvaluator
    {
        public GreaterThanOrEqual()
            : base(
                  ExpressionType.GreaterThanOrEqual,
                  Function,
                  FunctionUtils.ValidateBinaryNumberOrString,
                  FunctionUtils.VerifyNumberOrString)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.CultureInvariantDoubleConvert(args[0]) >= FunctionUtils.CultureInvariantDoubleConvert(args[1]);
        }
    }
}
