// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the result from adding numbers in a list.
    /// </summary>
    internal class Sum : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sum"/> class.
        /// </summary>
        public Sum()
            : base(ExpressionType.Sum, Evaluator(), ReturnType.Number, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var operands = FunctionUtils.ResolveListValue(args[0]).OfType<object>().ToList();
                            return operands.All(u => u.IsInteger()) ? operands.Sum(u => Convert.ToInt64(u, CultureInfo.InvariantCulture)) : operands.Sum(u => Convert.ToSingle(u, CultureInfo.InvariantCulture));
                        },
                        FunctionUtils.VerifyNumericList);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array);
        }
    }
}
