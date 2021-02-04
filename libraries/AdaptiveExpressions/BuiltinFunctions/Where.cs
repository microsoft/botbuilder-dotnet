// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Filter on each element and return the new collection of filtered elements which match a specific condition.
    /// </summary>
    internal class Where : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Where"/> class.
        /// </summary>
        public Where()
            : base(ExpressionType.Where, Evaluator, ReturnType.Array, FunctionUtils.ValidateLambdaExpression)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object result = null;
            string error;

            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state, options);
            if (error == null)
            {
                var list = FunctionUtils.ConvertToList(instance);
                if (list == null)
                {
                    error = $"{expression.Children[0]} is not a collection or structure object to run Where";
                }
                else
                {
                    result = new List<object>();
                    FunctionUtils.LambdaEvaluator(expression, state, options, list, (object currentItem, object r, string e) =>
                    {
                        if (FunctionUtils.IsLogicTrue(r) && e == null)
                        {
                            // add if only if it evaluates to true
                            ((List<object>)result).Add(currentItem);
                        }

                        return false;
                    });

                    if (!FunctionUtils.TryParseList(instance, out IList _))
                    {
                        // re-construct object
                        var jobjResult = new JObject();
                        foreach (var item in (List<object>)result)
                        {
                            FunctionUtils.TryAccessProperty(item, "key", out var keyVal);
                            FunctionUtils.TryAccessProperty(item, "value", out var val);
                            jobjResult.Add(keyVal as string, FunctionUtils.ConvertToJToken(val));
                        }

                        result = jobjResult;
                    }
                }
            }

            return (result, error);
        }
    }
}
