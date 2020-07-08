// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;

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

        public StringTransformEvaluator(string type, Func<IReadOnlyList<object>, Options, (object, string)> function)
            : base(type, Evaluator(function), ReturnType.String, expr => FunctionUtils.ValidateOrder(expr, new[] { ReturnType.String }, ReturnType.String))
        {
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, object> function)
        {
            return FunctionUtils.Apply(function, FunctionUtils.VerifyStringOrNull);
        }

        private static EvaluateExpressionDelegate Evaluator(Func<IReadOnlyList<object>, Options, (object, string)> function)
        {
            return FunctionUtils.ApplyWithOptionsAndError(function, FunctionUtils.VerifyStringOrNull);
        }
    }
}
