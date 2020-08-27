// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether at least one expression is true.
    /// Return true if at least one expression is true, or return false if all are false.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
    internal class Or : ExpressionEvaluator
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Or"/> class.
        /// </summary>
        public Or()
            : base(ExpressionType.Or, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object result = false;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state, new Options(options) { NullSubstitution = null });
                if (error == null)
                {
                    if (FunctionUtils.IsLogicTrue(result))
                    {
                        result = true;
                        break;
                    }
                }
                else
                {
                    // Interpret error as false and swallow errors
                    error = null;
                }
            }

            return (result, error);
        }
    }
}
