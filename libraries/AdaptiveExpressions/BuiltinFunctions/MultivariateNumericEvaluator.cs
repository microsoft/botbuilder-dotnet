// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class MultivariateNumericEvaluator : ExpressionEvaluator
    {
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
