// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the Boolean version of a value.
    /// </summary>
    internal class Bool : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bool"/> class.
        /// </summary>
        public Bool()
            : base(
                  ExpressionType.Bool,
                  Function,
                  FunctionUtils.ValidateUnary)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return FunctionUtils.IsLogicTrue(args[0]);
        }
    }
}
