// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Determines whether any element of a sequence satisfies a condition.
    /// </summary>
    internal class Any : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Any"/> class.
        /// </summary>
        public Any()
            : base(ExpressionType.Any, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateLambdaExpression)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            var result = false;
            string error;

            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state, options);
            if (error == null)
            {
                var list = FunctionUtils.ConvertToList(instance);
                if (list == null)
                {
                    error = $"{expression.Children[0]} is not a collection or structure object to run Any";
                }
                else
                {
                    FunctionUtils.LambdaEvaluator(expression, state, options, list, (object currentItem, object r, string e) =>
                    {
                        if (FunctionUtils.IsLogicTrue(r) && e == null)
                        {
                            result = true;
                            return true;
                        }

                        return false;
                    });
                }
            }

            return (result, error);
        }
    }
}
