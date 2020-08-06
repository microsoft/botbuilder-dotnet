// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Retrieve the value of the specified property from the JSON object.
    /// </summary>
    internal class GetProperty : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetProperty"/> class.
        /// </summary>
        public GetProperty()
            : base(ExpressionType.GetProperty, Evaluator, ReturnType.Object, Validator)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error;
            object firstItem;
            object property;

            var children = expression.Children;
            (firstItem, error) = children[0].TryEvaluate(state, options);
            if (error == null)
            {
                if (children.Length == 1)
                {
                    // get root value from memory
                    if (!(firstItem is string))
                    {
                        error = $"Single parameter {children[0]} is not a string.";
                    }
                    else
                    {
                        (value, error) = FunctionUtils.WrapGetValue(state, (string)firstItem, options);
                    }
                }
                else
                {
                    // get the peoperty value from the instance
                    (property, error) = children[1].TryEvaluate(state, options);
                    if (error == null)
                    {
                        (value, error) = FunctionUtils.WrapGetValue(MemoryFactory.Create(firstItem), (string)property, options);
                    }
                }
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Object);
        }
    }
}
