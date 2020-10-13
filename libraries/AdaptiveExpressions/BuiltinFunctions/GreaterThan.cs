// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
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
                  FunctionUtils.ValidateBinary,
                  FunctionUtils.VerifyNumberOrStringOrDateTime)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            if (args[0] is DateTime dt1 && args[1] is DateTime dt2)
            {
                return dt1 > dt2;
            }

            return FunctionUtils.CultureInvariantDoubleConvert(args[0]) > FunctionUtils.CultureInvariantDoubleConvert(args[1]);
        }
    }
}
