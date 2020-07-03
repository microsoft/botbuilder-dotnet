// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Sort elements in the collection in ascending order and return the sorted collection.
    /// </summary>
    public class SortBy : ExpressionEvaluator
    {
        public SortBy()
            : base(ExpressionType.SortBy, FunctionUtils.SortBy(false), ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array);
        }
    }
}
