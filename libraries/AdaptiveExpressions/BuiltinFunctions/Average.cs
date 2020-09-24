// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the number average of a numeric array.
    /// </summary>
    internal class Average : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Average"/> class.
        /// </summary>
        public Average()
            : base(ExpressionType.Average, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var operands = FunctionUtils.ResolveListValue(args[0]).OfType<object>().ToList();
                            return operands.Average(u => Convert.ToSingle(u, CultureInfo.InvariantCulture));
                        },
                        FunctionUtils.VerifyNumericList);
        }
    }
}
