// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Flatten an array into non-array values. You can optionally set the maximum depth to flatten to.
    /// </summary>
    internal class Flatten : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Flatten"/> class.
        /// </summary>
        public Flatten()
            : base(ExpressionType.Flatten, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            var depth = 100;
                            object result = null;
                            string error = null;
                            if (args.Count > 1)
                            {
                                (depth, error) = FunctionUtils.ParseInt32(args[1]);
                            }

                            if (error == null)
                            {
                                result = EvalFlatten((IEnumerable<object>)args[0], depth);
                            }

                            return (result, error);
                        });
        }

        private static IEnumerable<object> EvalFlatten(IEnumerable<object> list, int dept)
        {
            var result = list.ToList();
            if (dept < 1)
            {
                dept = 1;
            }

            for (var i = 0; i < dept; i++)
            {
                var hasArray = result.Any(u => FunctionUtils.TryParseList(u, out var _));
                if (hasArray)
                {
                    var tempList = new List<object>();
                    foreach (var item in result)
                    {
                        if (FunctionUtils.TryParseList(item, out var itemList))
                        {
                            foreach (var childItem in itemList)
                            {
                                tempList.Add(childItem);
                            }
                        }
                        else
                        {
                            tempList.Add(item);
                        }
                    }

                    result = tempList.ToList();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.Array);
        }
    }
}
