using System;
using System.Collections;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Range : ExpressionEvaluator
    {
        public Range()
            : base(ExpressionType.Range, Evaluator(), ReturnType.Array, FunctionUtils.ValidateBinaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            IList result = null;
                            var count = Convert.ToInt32(args[1]);
                            if (count <= 0)
                            {
                                error = $"The second parameter {args[1]} should be more than zero";
                            }
                            else
                            {
                                result = Enumerable.Range(Convert.ToInt32(args[0]), count).ToList();
                            }

                            return (result, error);
                        },
                        FunctionUtils.VerifyInteger);
        }
    }
}
