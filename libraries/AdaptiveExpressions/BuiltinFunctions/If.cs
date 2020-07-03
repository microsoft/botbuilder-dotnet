﻿using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class If : ExpressionEvaluator
    {
        public If(string alias = null)
            : base(alias ?? ExpressionType.If, Evaluator, ReturnType.Object, Validator)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object result;
            string error;
            (result, error) = expression.Children[0].TryEvaluate(state, new Options(options) { NullSubstitution = null });
            if (error == null && FunctionUtils.IsLogicTrue(result))
            {
                (result, error) = expression.Children[1].TryEvaluate(state, options);
            }
            else
            {
                // Swallow error and treat as false
                (result, error) = expression.Children[2].TryEvaluate(state, options);
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 3, 3);
        }
    }
}
