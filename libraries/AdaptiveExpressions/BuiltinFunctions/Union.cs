// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a collection that has all the items from the specified collections.
    /// To appear in the result, an item can appear in any collection passed to this function.
    /// If one or more items have the same name, the last item with that name appears in the result.
    /// </summary>
    internal class Union : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Union"/> class.
        /// </summary>
        public Union()
            : base(ExpressionType.Union, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var result = (IEnumerable<object>)args[0];
                            for (var i = 1; i < args.Count; i++)
                            {
                                var nextItem = (IEnumerable<object>)args[i];
                                result = result.Union(nextItem);
                            }

                            return result.ToList();
                        }, FunctionUtils.VerifyList);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, int.MaxValue, ReturnType.Array);
        }
    }
}
