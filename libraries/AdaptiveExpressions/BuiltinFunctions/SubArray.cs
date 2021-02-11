// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Returns a subarray from specified start and end positions. Index values start with the number 0.
    /// </summary>
    internal class SubArray : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubArray"/> class.
        /// </summary>
        public SubArray()
            : base(ExpressionType.SubArray, EvalSubArray, ReturnType.Array, Validator)
        {
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, new[] { ReturnType.Number }, ReturnType.Array, ReturnType.Number);
        }

        private static (object, string) EvalSubArray(Expression expression, object state, Options options)
        {
            object result = null;
            string error;
            object arr;
            (arr, error) = expression.Children[0].TryEvaluate(state, options);

            if (error == null)
            {
                if (FunctionUtils.TryParseList(arr, out var list))
                {
                    var startExpr = expression.Children[1];
                    int start;
                    (start, error) = startExpr.TryEvaluate<int>(state, options);
                    if (error == null)
                    {
                        if (error == null && (start < 0 || start > list.Count))
                        {
                            error = $"{startExpr}={start} which is out of range for {arr}";
                        }

                        if (error == null)
                        {
                            int end = 0;
                            if (expression.Children.Length == 2)
                            {
                                end = list.Count;
                            }
                            else
                            {
                                var endExpr = expression.Children[2];
                                (end, error) = endExpr.TryEvaluate<int>(state, options);
                                if (error == null && (end < 0 || end > list.Count))
                                {
                                    error = $"{endExpr}={end} which is out of range for {arr}";
                                }
                            }

                            if (error == null)
                            {
                                result = list.OfType<object>().Skip(start).Take(end - start).ToList();
                            }
                        }
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
