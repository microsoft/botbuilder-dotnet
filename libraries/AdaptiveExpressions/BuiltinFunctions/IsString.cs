// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is a string.
    /// </summary>
    internal class IsString : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsString"/> class.
        /// </summary>
        public IsString()
            : base(ExpressionType.IsString, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => args[0] != null && args[0].GetType() == typeof(string));
        }
    }
}
