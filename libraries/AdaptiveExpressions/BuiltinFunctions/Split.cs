// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return an array that contains substrings, separated by commas, based on the specified delimiter character in the original string.
    /// </summary>
    internal class Split : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Split"/> class.
        /// </summary>
        public Split()
            : base(ExpressionType.Split, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var inputStr = string.Empty;
                            var seperator = string.Empty;
                            if (args.Count == 1)
                            {
                                inputStr = FunctionUtils.ParseStringOrNull(args[0]);
                            }
                            else
                            {
                                inputStr = FunctionUtils.ParseStringOrNull(args[0]);
                                seperator = FunctionUtils.ParseStringOrNull(args[1]);
                            }

                            if (string.IsNullOrWhiteSpace(seperator))
                            {
                                return inputStr.Select(c => c.ToString()).ToArray();
                            }

                            return inputStr.Split(seperator.ToCharArray());
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 2, ReturnType.String);
        }
    }
}
