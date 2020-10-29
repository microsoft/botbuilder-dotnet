// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return items from the front of a collection or take the specific prefix from a string.
    /// </summary>
    internal class Take : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Take"/> class.
        /// </summary>
        public Take()
            : base(ExpressionType.Take, Evaluator, ReturnType.Array | ReturnType.String, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array | ReturnType.String, ReturnType.Number);
        }

        private static (object, string) Evaluator(Expression expression, object state, Options options)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state, options);
            if (error == null)
            {
                var arrIsList = FunctionUtils.TryParseList(arr, out var list);
                var arrIsStr = arr.GetType() == typeof(string);
                if (arrIsList || arrIsStr)
                {
                    int count;
                    var countExpr = expression.Children[1];
                    (count, error) = countExpr.TryEvaluate<int>(state, options);

                    if (error == null)
                    {
                        if (arrIsList)
                        {
                            count = Math.Max(Math.Min(list.Count, count), 0);
                            result = list.OfType<object>().Take(count).ToList();
                        }
                        else
                        {
                            count = Math.Max(Math.Min(arr.ToString().Length, count), 0);
                            result = arr.ToString().Substring(0, count);
                        }
                    }
                }
                else
                {
                    error = $"{expression.Children[0]} is not array or string.";
                }
            }

            return (result, error);
        }
    }
}
