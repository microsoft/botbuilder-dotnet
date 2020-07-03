using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class GetProperty : ExpressionEvaluator
    {
        public GetProperty(string alias = null)
            : base(alias ?? ExpressionType.GetProperty, Evaluator, ReturnType.Object, Validator)
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
