using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Split : ExpressionEvaluator
    {
        public Split(string alias = null)
            : base(alias ?? ExpressionType.Split, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            var inputStr = string.Empty;
                            var seperator = string.Empty;
                            if (args.Count == 1)
                            {
                                inputStr = FunctionUtils.ParseStringOrNull(args[0]);
                            }
                            else
                            {
                                inputStr = FunctionUtils.ParseStringOrNull(args[0]);
                                seperator = FunctionUtils.ParseStringOrNull(args[1]);
                            }

                            if (seperator == string.Empty)
                            {
                                return inputStr.Select(c => c.ToString()).ToArray();
                            }

                            return inputStr.Split(seperator.ToCharArray());
                        }, FunctionUtils.VerifyStringOrNull);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 2, ReturnType.String);
        }
    }
}
