// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Evaluator that transforms a number to another number.
    /// </summary>
    public class NumberTransformEvaluator : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumberTransformEvaluator"/> class.
        /// </summary>
        /// <param name="type">Name of the function.</param>
        /// <param name="function"> The number tranform function, it takes a list of objects as input and returns an object.</param>
        public NumberTransformEvaluator(string type, Func<IReadOnlyList<object>, object> function)
            : base(type, Evaluator(function), ReturnType.Number, FunctionUtils.ValidateUnaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, object> function)
        {
            return FunctionUtils.Apply(function, FunctionUtils.VerifyNumber);
        }
    }
}
