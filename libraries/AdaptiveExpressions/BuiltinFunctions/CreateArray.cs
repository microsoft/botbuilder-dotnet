using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class CreateArray : ExpressionEvaluator
    {
        public CreateArray()
            : base(ExpressionType.CreateArray, Evaluator(), ReturnType.Array)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => new List<object>(args));
        }
    }
}
