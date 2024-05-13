// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Reverses the order of the elements in a String or Array.
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
                            object result = null;
                            string error = null;

                            if (args[0] is string string0)
                            {
                                result = new string(string0.Reverse().ToArray());
                            }
                            else if (args[0] is IList list)
                            {
                                result = list.OfType<object>().Reverse().ToList();
                            }
                            else
                            {
                                error = $"{args[0]} is not a string or list.";
                            }

                            return (result, error);
                        }, FunctionUtils.VerifyContainer);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String | ReturnType.Array);
        }
    }
}
