using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class And : ExpressionEvaluator
    {
        public And(string alias = null))
            : base(ExpressionType.And, EvalAnd, ReturnType.Boolean, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static (object value, string error) EvalAnd(Expression expression, IMemory state, Options options)
        {
            object result = true;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state, new Options(options) { NullSubstitution = null });
                if (error == null)
                {
                    if (FunctionUtils.IsLogicTrue(result))
                    {
                        result = true;
                    }
                    else
                    {
                        result = false;
                        break;
                    }
                }
                else
                {
                    // We interpret any error as false and swallow the error
                    result = false;
                    error = null;
                    break;
                }
            }

            return (result, error);
        }
    }
}
