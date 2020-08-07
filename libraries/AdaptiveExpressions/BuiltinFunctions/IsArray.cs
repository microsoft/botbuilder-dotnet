// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is an array.
    /// </summary>
    internal class IsArray : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsArray"/> class.
        /// </summary>
        public IsArray()
            : base(ExpressionType.IsArray, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.TryParseList(args[0], out var _));
        }
    }
}
