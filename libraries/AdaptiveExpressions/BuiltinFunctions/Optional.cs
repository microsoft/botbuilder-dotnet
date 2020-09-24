// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// For the MostSpecificSelector, this is a short hand so that instead of having to do A &amp; B and A you can do A &amp; optional(B) to mean the same thing.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
    internal class Optional : ExpressionEvaluator
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Optional"/> class.
        /// </summary>
        public Optional()
            : base(ExpressionType.Optional, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnaryBoolean)
        {
            Negation = this;
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            throw new NotImplementedException();
        }
    }
}
