using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Take : ExpressionEvaluator
    {
        public Take(string alias = null)
            : base(alias ?? ExpressionType.Take, EvalTake, ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array, ReturnType.Number);
        }

        private static (object, string) EvalTake(Expression expression, object state, Options options)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state, options);
            if (error == null)
            {
                var arrIsList = FunctionUtils.TryParseList(arr, out var list);
                var arrIsStr = arr.GetType() == typeof(string);
                if (arrIsList || arrIsStr)
                {
                    int count;
                    var countExpr = expression.Children[1];
                    (count, error) = countExpr.TryEvaluate<int>(state, options);
                    if (error == null)
                    {
                        if (arrIsList)
                        {
                            if (count < 0 || count >= list.Count)
                            {
                                error = $"{countExpr}={count} which is out of range for {arr}";
                            }
                            else
                            {
                                result = list.OfType<object>().Take(count).ToList();
                            }
                        }
                        else
                        {
                            if (count < 0 || count > list.Count)
                            {
                                error = $"{countExpr}={count} which is out of range for {arr}";
                            }
                            else
                            {
                                result = arr.ToString().Substring(0, count);
                            }
                        }
                    }
                }
                else
                {
                    error = $"{expression.Children[0]} is not array or string.";
                }
            }

            return (result, error);
        }
    }
}
