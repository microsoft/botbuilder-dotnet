// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether all expressions are true. Return true if all expressions are true,
    /// or return false if at least one expression is false.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
    internal class And : ExpressionEvaluator
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="And"/> class.
        /// </summary>
        public And()
            : base(ExpressionType.And, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object result = true;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state, new Options(options) { NullSubstitution = null });
                if (error == null)
                {
                    if (FunctionUtils.IsLogicTrue(result))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
                else
                {
                    // We interpret any error as false and swallow the error
                    result = false;
                    error = null;
                    break;
                }
            }

            return (result, error);
        }
    }
}
