using System;
using System.Collections;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class LastIndexOf : ExpressionEvaluator
    {
        public LastIndexOf()
            : base(ExpressionType.LastIndexOf, EvalLastIndexOf, ReturnType.Number, Validator)
        {
        }

        private static (object value, string error) EvalLastIndexOf(Expression expression, IMemory state, Options options)
        {
            object result = -1;
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0] is string || args[0] == null)
                {
                    if (args[1] is string || args[1] == null)
                    {
                        result = FunctionUtils.ParseStringOrNull(args[0]).IndexOf(FunctionUtils.ParseStringOrNull(args[1]));
                    }
                    else
                    {
                        error = $"Can only look for indexof string in {expression}";
                    }
                }
                else if (FunctionUtils.TryParseList(args[0], out IList list))
                {
                    result = FunctionUtils.ResolveListValue(list).OfType<object>().ToList().LastIndexOf(args[1]);
                }
                else
                {
                    error = $"{expression} works only on string or list.";
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array | ReturnType.String, ReturnType.Object);
        }
    }
}
