// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the length of a string.
    /// </summary>
    internal class Length : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Length"/> class.
        /// </summary>
        public Length()
            : base(ExpressionType.Length, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnaryString)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var result = 0;
                            if (args[0] is string str)
                            {
                                result = str.Length;
                            }
                            else
                            {
                                result = 0;
                            }

                            return result;
                        }, FunctionUtils.VerifyStringOrNull);
        }
    }
}
