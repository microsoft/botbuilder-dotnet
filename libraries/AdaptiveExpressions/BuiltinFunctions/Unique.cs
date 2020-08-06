// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Remove all duplicates from an array.
    /// </summary>
    internal class Unique : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Unique"/> class.
        /// </summary>
        public Unique()
            : base(ExpressionType.Unique, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            return ((IEnumerable<object>)args[0]).Distinct().ToList();
                        }, FunctionUtils.VerifyList);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array);
        }
    }
}
