// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Evaluator that transforms a string to another string.
    /// </summary>
    public class StringTransformEvaluator : ExpressionEvaluator
    {
        public StringTransformEvaluator(string type, Func<IReadOnlyList<object>, object> function)
            : base(type, Evaluator(function), ReturnType.String, FunctionUtils.ValidateUnaryString)
        {
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, object> function)
        {
            return FunctionUtils.Apply(function, FunctionUtils.VerifyStringOrNull);
        }
    }
}
