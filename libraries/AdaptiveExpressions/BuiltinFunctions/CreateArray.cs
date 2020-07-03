using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class CreateArray : ExpressionEvaluator
    {
        public CreateArray(string alias = null)
            : base(alias ?? ExpressionType.CreateArray, Evaluator(), ReturnType.Array)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => new List<object>(args));
        }
    }
}
