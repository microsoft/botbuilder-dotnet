using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Ticks : ExpressionEvaluator
    {
        public Ticks()
            : base(ExpressionType.Ticks, EvalTicks, ReturnType.Number, Validator)
        {
        }

        private static (object value, string error) EvalTicks(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (value, error) = FunctionUtils.TicksWithError(args[0]);
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 1, ReturnType.String);
        }
    }
}
