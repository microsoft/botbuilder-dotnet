// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Numeric operators that can have 2 or more args.
    /// </summary>
    public class MultivariateNumericEvaluator : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultivariateNumericEvaluator"/> class.
        /// </summary>
        /// <param name="type">Name of the function.</param>
        /// <param name="function"> The multivariate numeric function, it takes a list of objects as input and returns an object.</param>
        /// <param name = "verify" > Optional function to verify each child's result.</param>
        public MultivariateNumericEvaluator(string type, Func<IReadOnlyList<object>, object> function, FunctionUtils.VerifyExpression verify = null)
            : base(type, Evaluator(function, verify), ReturnType.Number, FunctionUtils.ValidateTwoOrMoreThanTwoNumbers)
        {
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, object> function, FunctionUtils.VerifyExpression verify = null)
        {
            return FunctionUtils.ApplySequence(function, verify ?? FunctionUtils.VerifyNumber);
        }
    }
}
