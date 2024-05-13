// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether an expression is false.
    /// Return true if the expression is false, or return false if true.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
    internal class Not : ExpressionEvaluator
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Not"/> class.
        /// </summary>
        public Not()
            : base(ExpressionType.Not, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object result;
            string error;
            (result, error) = expression.Children[0].TryEvaluate(state, new Options(options) { NullSubstitution = null });
            if (error == null)
            {
                result = !FunctionUtils.IsLogicTrue(result);
            }
            else
            {
                error = null;
                result = true;
            }

            return (result, error);
        }
    }
}
