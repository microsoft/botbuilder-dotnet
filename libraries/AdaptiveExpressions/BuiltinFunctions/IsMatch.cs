// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given string is matches a specified regular expression pattern.
    /// </summary>
    internal class IsMatch : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsMatch"/> class.
        /// </summary>
        public IsMatch()
            : base(ExpressionType.IsMatch, Evaluator(), ReturnType.Boolean, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            var value = false;
                            string error = null;

                            string inputString = args[0]?.ToString();
                            if (string.IsNullOrEmpty(inputString))
                            {
                                value = false;
                                error = "regular expression is empty.";
                            }
                            else
                            {
                                var regex = CommonRegex.CreateRegex(args[1].ToString());
                                value = regex.IsMatch(inputString);
                            }

                            return (value, error);
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, 2, ReturnType.String);

            var second = expression.Children[1];
            if (second.ReturnType == ReturnType.String && second.Type == ExpressionType.Constant)
            {
                CommonRegex.CreateRegex((second as Constant).Value.ToString());
            }
        }
    }
}
