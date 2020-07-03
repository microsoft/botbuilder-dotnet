﻿using System;
using System.Collections;
using System.Linq;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Join : ExpressionEvaluator
    {
        public Join(string alias = null)
            : base(alias ?? ExpressionType.Join, EvalJoin, ReturnType.String, Validator)
        {
        }

        private static (object value, string error) EvalJoin(Expression expression, IMemory state, Options options)
        {
            object result = null;
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (!FunctionUtils.TryParseList(args[0], out IList list))
                {
                    error = $"{expression.Children[0]} evaluates to {args[0]} which is not a list.";
                }
                else
                {
                    if (args.Count == 2)
                    {
                        result = string.Join(args[1].ToString(), list.OfType<object>().Select(x => x.ToString()));
                    }
                    else
                    {
                        if (list.Count < 3)
                        {
                            result = string.Join(args[2].ToString(), list.OfType<object>().Select(x => x.ToString()));
                        }
                        else
                        {
                            var firstPart = string.Join(args[1].ToString(), list.OfType<object>().TakeWhile(o => o != null && o != list.OfType<object>().LastOrDefault()));
                            result = firstPart + args[2] + list.OfType<object>().Last().ToString();
                        }
                    }
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.String }, ReturnType.Array, ReturnType.String);
        }
    }
}
