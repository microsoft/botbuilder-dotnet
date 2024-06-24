// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Nodes;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
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

        private static (object, string) Evaluator(Expression expression, IMemory state, Options options)
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
                        tempList.Add(MakeIndexEntry<object>(i, list[i]));
                    }

                    result = tempList;
                }
                else if (instance is JsonObject jobj)
                {
                    result = Object2List(jobj);
                }
                else if (state.SerializeToNode(instance) is JsonObject jsonObject)
                {
                    result = Object2List(jsonObject);
                }
                else
                {
                    error = $"{expression.Children[0]} is not array or object..";
                }
            }

            return (result, error);
        }

        private static List<object> Object2List(JsonObject jobj)
        {
            var tempList = new List<object>();
            foreach (var item in jobj)
            {
                tempList.Add(MakeIndexEntry<JsonNode>(item.Key, item.Value));
            }

            return tempList;
        }

        private static Dictionary<string, TValue> MakeIndexEntry<TValue>(TValue index, TValue value)
        {
            return new Dictionary<string, TValue>
            {
                { "index", index },
                { "value", value }
            };
        }
    }
}
