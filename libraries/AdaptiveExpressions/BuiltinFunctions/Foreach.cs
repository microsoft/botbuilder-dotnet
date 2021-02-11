// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Operate on each element and return the new collection.
    /// </summary>
    internal class Foreach : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Foreach"/> class.
        /// </summary>
        public Foreach()
            : base(ExpressionType.Foreach, FunctionUtils.Foreach, ReturnType.Array, FunctionUtils.ValidateForeach)
        {
        }
    }
}
