// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return the number of items in a collection.
    /// </summary>
    internal class Count : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Count"/> class.
        /// </summary>
        public Count()
            : base(ExpressionType.Count, Evaluator(), ReturnType.Number, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            object count = null;
                            if (args[0] is string string0)
                            {
                                count = string0.Length;
                            }
                            else if (FunctionUtils.TryAsList(args[0], out var list))
                            {
                                count = FunctionUtils.GetListCount(list);
                            }

                            return count;
                        }, FunctionUtils.VerifyContainer);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String | ReturnType.Array);
        }
    }
}
