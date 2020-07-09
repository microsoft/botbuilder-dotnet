// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Operate on each element and return the new collection of transformed elements.
    /// </summary>
    public class Select : ExpressionEvaluator
    {
        public Select()
            : base(ExpressionType.Select, FunctionUtils.Foreach, ReturnType.Array, FunctionUtils.ValidateForeach)
        {
        }
    }
}
