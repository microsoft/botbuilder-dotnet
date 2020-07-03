// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Optional : ExpressionEvaluator
    {
        public Optional(string alias = null)
            : base(alias ?? ExpressionType.Optional, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnaryBoolean)
        {
            Negation = this;
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            throw new NotImplementedException();
        }
    }
}
