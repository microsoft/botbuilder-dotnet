// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Replace a substring with the specified string, and return the result string.
    /// This function is case-insensitive.
    /// </summary>
    internal class ReplaceIgnoreCase : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplaceIgnoreCase"/> class.
        /// </summary>
        public ReplaceIgnoreCase()
            : base(ExpressionType.ReplaceIgnoreCase, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            string result = null;
                            var inputStr = FunctionUtils.ParseStringOrNull(args[0]);
                            var oldStr = FunctionUtils.ParseStringOrNull(args[1]);
                            if (oldStr.Length == 0)
                            {
                                error = $"{args[1]} the oldValue in replace function should be a string with at least length 1.";
                            }

                            string newStr = FunctionUtils.ParseStringOrNull(args[2]);
                            if (error == null)
                            {
                                result = Regex.Replace(inputStr, oldStr, newStr, RegexOptions.IgnoreCase);
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
