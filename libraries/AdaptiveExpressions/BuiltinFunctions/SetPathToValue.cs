// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Retrieve the value of the specified property from the JSON object.
    /// </summary>
    internal class SetPathToValue : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetPathToValue"/> class.
        /// </summary>
        public SetPathToValue()
            : base(ExpressionType.SetPathToValue, Evaluator, ReturnType.Object, FunctionUtils.ValidateBinary)
        {
        }

        private static (object value, string error) Evaluator(Expression expr, IMemory state, Options options)
        {
            var (path, left, error) = FunctionUtils.TryAccumulatePath(expr.Children[0], state, options);

            if (error != null)
            {
                return (null, error);
            }

            if (left != null)
            {
                // the expression can't be fully merged as a path
                return (null, $"{expr.Children[0]} is not a valid path to set value");
            }

            var (value, err) = expr.Children[1].TryEvaluate(state, options);
            if (err != null)
            {
                return (null, err);
            }

            state.SetValue(path, value);
            return (value, null);
        }
    }
}
