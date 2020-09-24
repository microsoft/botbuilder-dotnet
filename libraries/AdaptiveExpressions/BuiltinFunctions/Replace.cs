// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Replace a substring with the specified string,
    /// and return the result string.
    /// This function is case-sensitive.
    /// </summary>
    internal class Replace : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Replace"/> class.
        /// </summary>
        public Replace()
            : base(ExpressionType.Replace, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            string result = null;
                            string inputStr = FunctionUtils.ParseStringOrNull(args[0]);
                            string oldStr = FunctionUtils.ParseStringOrNull(args[1]);
                            if (oldStr.Length == 0)
                            {
                                error = $"{args[1]} the oldValue in replace function should be a string with at least length 1.";
                            }

                            string newStr = FunctionUtils.ParseStringOrNull(args[2]);
                            if (error == null)
                            {
                                result = inputStr.Replace(oldStr, newStr);
                            }

                            return (result, error);
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 3, 3, ReturnType.String);
        }
    }
}
