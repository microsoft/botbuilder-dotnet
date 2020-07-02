using System;
using System.Collections;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class First : ExpressionEvaluator
    {
        public First()
            : base(ExpressionType.First, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            object first = null;
                            if (args[0] is string string0 && string0.Length > 0)
                            {
                                first = string0.First().ToString();
                            }
                            else if (FunctionUtils.TryParseList(args[0], out IList list) && list.Count > 0)
                            {
                                first = FunctionUtils.AccessIndex(list, 0).value;
                            }

                            return first;
                        });
        }
    }
}
