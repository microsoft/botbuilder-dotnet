using System;
using System.Collections.Generic;
using System.Linq;
using AdaptiveExpressions.Memory;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsPresent : ExpressionEvaluator
    {
        public IsPresent(string alias = null)
            : base(alias ?? ExpressionType.IsPresent, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateUnary)
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
                value = parsed.Now != null;
            }

            return (value, error);
        }
    }
}
