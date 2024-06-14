﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using Microsoft.AdaptiveExpressions.Core.Memory;

namespace Microsoft.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Remove items from the front of a collection, and return all the other items.
    /// </summary>
    internal class Skip : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Skip"/> class.
        /// </summary>
        public Skip()
            : base(ExpressionType.Skip, EvalSkip, ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array, ReturnType.Number);
        }

        private static (object value, string error) EvalSkip(Expression expression, IMemory state, Options options)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state, options);

            if (error == null)
            {
                if (FunctionUtils.TryParseList(arr, out var list))
                {
                    int start;
                    var startExpr = expression.Children[1];
                    (start, error) = startExpr.TryEvaluate<int>(state, options);

                    if (error == null)
                    {
                        // If start exceeds the number of elements, an empty IEnumerable<T> is returned.
                        // If start is less than or equal to zero, all elements of source are returned.
                        start = Math.Max(Math.Min(list.Count, start), 0);
                        result = list.OfType<object>().Skip(start).ToList();
                    }
                }
                else
                {
                    error = $"{expression.Children[0]} is not array.";
                }
            }

            return (result, error);
        }
    }
}
