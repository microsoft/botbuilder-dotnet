// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Turn an array or object into an array of objects with index (current index) and value properties.
    /// For arrays, the index is the position in the array.
    /// For objects, it is the key for the value.
    /// </summary>
    internal class IndicesAndValues : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndicesAndValues"/> class.
        /// </summary>
        public IndicesAndValues()
            : base(ExpressionType.IndicesAndValues, Evaluator, ReturnType.Array, FunctionUtils.ValidateUnary)
        {
        }

        private static (object, string) Evaluator(Expression expression, object state, Options options)
        {
            object result = null;
            string error;
            object instance;
            (instance, error) = expression.Children[0].TryEvaluate(state, options);
            if (error == null)
            {
                if (FunctionUtils.TryParseList(instance, out var list))
                {
                    var tempList = new List<object>();
                    for (var i = 0; i < list.Count; i++)
                    {
                        tempList.Add(new { index = i, value = list[i] });
                    }

                    result = tempList;
                }
                else if (instance is JObject jobj)
                {
                    result = Object2List(jobj);
                }
                else if (FunctionUtils.ConvertToJToken(instance) is JObject jobject)
                {
                    result = Object2List(jobject);
                }
                else
                {
                    error = $"{expression.Children[0]} is not array or object..";
                }
            }

            return (result, error);
        }

        private static List<object> Object2List(JObject jobj)
        {
            var tempList = new List<object>();
            foreach (var item in jobj)
            {
                tempList.Add(new { index = item.Key, value = item.Value });
            }

            return tempList;
        }
    }
}
