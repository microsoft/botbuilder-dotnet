// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    ///  Mark a clause so that MostSpecificSelector will ignore it.
    ///  MostSpecificSelector considers A &amp; B to be more specific than A, but some clauses are unique and incomparable.
    /// </summary>
    public class Ignore : ExpressionEvaluator
    {
        public Ignore()
            : base(ExpressionType.Ignore, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnaryBoolean)
        {
            Negation = this;
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            return expression.Children[0].TryEvaluate(state, options);
        }
    }
}
