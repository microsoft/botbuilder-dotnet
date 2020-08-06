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
        /// <summary>
        /// Initializes a new instance of the <see cref="StringTransformEvaluator"/> class.
        /// </summary>
        /// <param name="type">Name of the built-in function.</param>
        /// <param name="function">The string transformation function, it takes a list of objects and returns an object.</param>
        public StringTransformEvaluator(string type, Func<IReadOnlyList<object>, object> function)
            : base(type, Evaluator(function), ReturnType.String, FunctionUtils.ValidateUnaryString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringTransformEvaluator"/> class.
        /// </summary>
        /// <param name="type">Name of the built-in function.</param>
        /// <param name="function">The string transformation function, it takes a list of objects, an <see cref="Options"/> instance and returns a (object, string) tuple.</param>
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
