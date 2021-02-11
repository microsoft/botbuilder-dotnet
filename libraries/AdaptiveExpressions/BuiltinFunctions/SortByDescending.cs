// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Sort elements in the collection in descending order, and return the sorted collection.
    /// </summary>
    internal class SortByDescending : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SortByDescending"/> class.
        /// </summary>
        public SortByDescending()
            : base(ExpressionType.SortByDescending, FunctionUtils.SortBy(true), ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array);
        }
    }
}
