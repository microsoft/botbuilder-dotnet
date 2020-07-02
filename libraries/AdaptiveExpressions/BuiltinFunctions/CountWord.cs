using System;
using System.Text.RegularExpressions;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class CountWord : ExpressionEvaluator
    {
        public CountWord()
            : base(ExpressionType.CountWord, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnaryString)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            if (args[0] is string)
                            {
                                return Regex.Split(args[0].ToString().Trim(), @"\s{1,}").Length;
                            }
                            else
                            {
                                return 0;
                            }
                        }, FunctionUtils.VerifyStringOrNull);
        }
    }
}
