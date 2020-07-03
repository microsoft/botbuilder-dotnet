﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsDefinite : ExpressionEvaluator
    {
        public IsDefinite(string alias = null)
            : base(alias ?? ExpressionType.IsDefinite, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnary)
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
                value = parsed != null && parsed.Year != null && parsed.Month != null && parsed.DayOfMonth != null;
            }

            return (value, error);
        }
    }
}
