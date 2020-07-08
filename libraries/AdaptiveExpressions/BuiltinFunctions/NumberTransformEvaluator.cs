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
