// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Evaluator that transforms a date-time to another date-time.
    /// </summary>
    public class TimeTransformEvaluator : ExpressionEvaluator
    {
        public TimeTransformEvaluator(string type, Func<DateTime, int, DateTime> function)
            : base(type, Evaluator(function), ReturnType.String, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.String, ReturnType.Number);
        }

        private static EvaluateExpressionDelegate Evaluator(Func<DateTime, int, DateTime> function)
        {
            return (expression, state, options) =>
            {
                object value = null;
                string error = null;
                IReadOnlyList<object> args;
                (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
                if (error == null)
                {
                    if (args[1].IsInteger())
                    {
                        var formatString = (args.Count == 3 && args[2] is string string1) ? string1 : FunctionUtils.DefaultDateTimeFormat;
                        (value, error) = FunctionUtils.NormalizeToDateTime(args[0], dt => 
                        {
                            var result = dt;
                            var (interval, error) = FunctionUtils.ParseInt32(args[1]);
                            if (error == null)
                            {
                                result = function(dt, interval);
                            }

                            return (result, error);
                        });

                        if (error == null)
                        {
                            value = Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToString(formatString, CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        error = $"{expression} should contain an ISO format timestamp and a time interval integer.";
                    }
                }

                return (value, error);
            };
        }
    }
}
