﻿using System;
using System.Globalization;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class FormatTicks : ExpressionEvaluator
    {
        public FormatTicks(string alias = null)
            : base(alias ?? ExpressionType.FormatTicks, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            var timestamp = args[0];
                            if (timestamp.IsInteger())
                            {
                                var ticks = Convert.ToInt64(timestamp);
                                var dateTime = new DateTime(ticks);
                                result = dateTime.ToString(args.Count() == 2 ? args[1].ToString() : FunctionUtils.DefaultDateTimeFormat, CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                error = $"formatTicks first arugment {timestamp} must be an integer";
                            }

                            return (result, error);
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Number);
        }
    }
}
