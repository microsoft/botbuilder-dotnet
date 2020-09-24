// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Evaluates an expression for truthiness.
    /// </summary>
    internal class Exists : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exists"/> class.
        /// </summary>
        public Exists()
            : base(
                  ExpressionType.Exists,
                  Function,
                  FunctionUtils.ValidateUnary,
                  FunctionUtils.VerifyNotNull)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return args[0] != null;
        }
    }
}
