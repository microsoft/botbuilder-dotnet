// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether a string ends with a specific substring. Return true if the substring is found,
    /// or return false if not found.
    /// This function is case-insensitive.
    /// </summary>
    internal class EndsWith : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndsWith"/> class.
        /// </summary>
        public EndsWith()
            : base(ExpressionType.EndsWith, Evaluator(), ReturnType.Boolean, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var rawStr = FunctionUtils.ParseStringOrNull(args[0]);
                            var seekStr = FunctionUtils.ParseStringOrNull(args[1]);
                            return rawStr.EndsWith(seekStr, StringComparison.Ordinal);
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 2, ReturnType.String);
        }
    }
}
