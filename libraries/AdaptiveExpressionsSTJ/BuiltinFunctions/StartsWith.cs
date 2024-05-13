// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether a string starts with a specific substring. Return true if the substring is found, or return false if not found.
    /// This function is case-insensitive.
    /// </summary>
    internal class StartsWith : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartsWith"/> class.
        /// </summary>
        public StartsWith()
            : base(ExpressionType.StartsWith, Evaluator(), ReturnType.Boolean, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            string rawStr = FunctionUtils.ParseStringOrNull(args[0]);
                            string seekStr = FunctionUtils.ParseStringOrNull(args[1]);
                            return rawStr.StartsWith(seekStr, StringComparison.Ordinal);
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 2, ReturnType.String);
        }
    }
}
