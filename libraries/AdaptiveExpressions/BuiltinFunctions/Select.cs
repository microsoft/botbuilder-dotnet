// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Operate on each element and return the new collection of transformed elements.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
    internal class Select : ExpressionEvaluator
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Select"/> class.
        /// </summary>
        public Select()
            : base(ExpressionType.Select, FunctionUtils.Foreach, ReturnType.Array, FunctionUtils.ValidateLambdaExpression)
        {
        }
    }
}
