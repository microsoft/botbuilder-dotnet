using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Skip : ExpressionEvaluator
    {
        public Skip(string alias = null)
            : base(alias ?? ExpressionType.Skip, EvalSkip, ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array, ReturnType.Number);
        }

        private static (object value, string error) EvalSkip(Expression expression, object state, Options options)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state, options);

            if (error == null)
            {
                if (FunctionUtils.TryParseList(arr, out var list))
                {
                    int start = 0;
                    var startExpr = expression.Children[1];
                    (start, error) = startExpr.TryEvaluate<int>(state, options);
                    if (error == null && (start < 0 || start >= list.Count))
                    {
                        error = $"{startExpr}={start} which is out of range for {arr}";
                    }

                    if (error == null)
                    {
                        result = list.OfType<object>().Skip(start).ToList();
                    }
                }
                else
                {
                    error = $"{expression.Children[0]} is not array.";
                }
            }

            return (result, error);
        }
    }
}
