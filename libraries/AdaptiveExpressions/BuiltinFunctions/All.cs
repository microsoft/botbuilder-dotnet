// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Determines whether all elements of a sequence satisfy a condition.
    /// </summary>
    internal class All : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="All"/> class.
        /// </summary>
        public All()
            : base(ExpressionType.All, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateLambdaExpression)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            var result = true;
            string error;

            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state, options);
            if (error == null)
            {
                var list = FunctionUtils.ConvertToList(instance);
                if (list == null)
                {
                    error = $"{expression.Children[0]} is not a collection or structure object to run All";
                }
                else
                {
                    FunctionUtils.LambdaEvaluator(expression, state, options, list, (object currentItem, object r, string e) =>
                    {
                        if (!FunctionUtils.IsLogicTrue(r) || e != null)
                        {
                            result = false;
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
