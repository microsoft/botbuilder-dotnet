﻿using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class DayOfWeek : ExpressionEvaluator
    {
        public DayOfWeek(string alias = null)
            : base(alias ?? ExpressionType.DayOfWeek, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => Convert.ToInt32(dt.DayOfWeek)));
        }
    }
}
