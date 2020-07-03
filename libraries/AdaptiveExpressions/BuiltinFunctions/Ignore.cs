// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Ignore : ExpressionEvaluator
    {
        public Ignore()
            : base(ExpressionType.Optional, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnaryBoolean)
        {
            Negation = this;
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            return expression.Children[0].TryEvaluate(state, options);
        }
    }
}
