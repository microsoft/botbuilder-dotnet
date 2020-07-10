// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Numeric operators that can have 1 or more args.
    /// </summary>
    public class NumericEvaluator : ExpressionEvaluator
    {
        public NumericEvaluator(string type, Func<IReadOnlyList<object>, object> function)
            : base(type, Evaluator(function), ReturnType.Number, FunctionUtils.ValidateNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, object> function)
        {
            return FunctionUtils.ApplySequence(function, FunctionUtils.VerifyNumber);
        }
    }
}
