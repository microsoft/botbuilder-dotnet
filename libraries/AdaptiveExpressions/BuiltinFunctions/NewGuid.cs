﻿using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class NewGuid : ExpressionEvaluator
    {
        public NewGuid(string alias = null)
            : base(alias ?? ExpressionType.NewGuid, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Guid.NewGuid().ToString());
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 0, 0);
        }
    }
}
