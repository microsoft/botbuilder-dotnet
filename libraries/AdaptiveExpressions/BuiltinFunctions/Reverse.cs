// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Reverses the order of the elements in the String/Array.
    /// </summary>
    internal class Reverse : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Reverse"/> class.
        /// </summary>
        public Reverse()
            : base(ExpressionType.Reverse, Evaluator(), ReturnType.String | ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            var result = args[0];
                            if (args[0] is string string0)
                            {
                                return (new string(string0.Reverse().ToArray()), null);
                            }
                            else if (args[0] is IList list)
                            {
                                return (list.OfType<object>().Reverse().ToList(), null);
                            }

                            return (null, $"{args[0]} is not a string or list.");
                        }, FunctionUtils.VerifyContainer);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String | ReturnType.Array);
        }
    }
}
