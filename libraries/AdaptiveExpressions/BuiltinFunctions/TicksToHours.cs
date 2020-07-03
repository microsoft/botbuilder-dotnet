using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class TicksToHours : ExpressionEvaluator
    {
        private const long TicksPerHour = 60 * 60 * 10000000L;

        public TicksToHours(string alias = null)
            : base(alias ?? ExpressionType.TicksToHours, EvalTicksToHours, ReturnType.Number, FunctionUtils.ValidateUnaryNumber)
        {
        }

        private static (object value, string error) EvalTicksToHours(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0].IsInteger())
                {
                    value = Convert.ToDouble(args[0]) / TicksPerHour;
                }
                else
                {
                    error = $"{expression} should contain an integer of ticks";
                }
            }

            return (value, error);
        }
    }
}
