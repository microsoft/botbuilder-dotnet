using System;
using System.Collections;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Count : ExpressionEvaluator
    {
        public Count(string alias = null)
            : base(alias ?? ExpressionType.Count, Evaluator(), ReturnType.Number, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            object count = null;
                            if (args[0] is string string0)
                            {
                                count = string0.Length;
                            }
                            else if (args[0] is IList list)
                            {
                                count = list.Count;
                            }

                            return count;
                        }, FunctionUtils.VerifyContainer);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String | ReturnType.Array);
        }
    }
}
