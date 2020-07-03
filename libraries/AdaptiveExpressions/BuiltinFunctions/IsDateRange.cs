﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsDateRange : ExpressionEvaluator
    {
        public IsDateRange(string alias = null)
            : base(alias ?? ExpressionType.IsDateRange, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            TimexProperty parsed = null;
            bool? value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (parsed, error) = FunctionUtils.ParseTimexProperty(args[0]);
            }

            if (error == null)
            {
                value = (parsed.Year != null && parsed.DayOfMonth == null) ||
                                    (parsed.Year != null && parsed.Month != null && parsed.DayOfMonth == null) ||
                                    (parsed.Month != null && parsed.DayOfMonth == null) ||
                                    parsed.Season != null || parsed.WeekOfYear != null || parsed.WeekOfMonth != null;
            }

            return (value, error);
        }
    }
}
