// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Merge multiple object(json) into one object(json).
    /// If the item is array, the elements of the array are merged as well.
    /// </summary>
    internal class Merge : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Merge"/> class.
        /// </summary>
        public Merge()
            : base(ExpressionType.Merge, Evaluator(), ReturnType.Object, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                (args, state) =>
                {
                    var result = new JsonObject();

                    foreach (var arg in args)
                    {
                        var (list, itemError) = ParseToObjectList(arg, state);

                        if (itemError != null)
                        {
                            return (null, itemError);
                        }

                        foreach (var item in list)
                        {
                            result.Merge(item);
                        }
                    }

                    return (result, null);
                });
        }

        private static (List<JsonObject>, string) ParseToObjectList(object arg, IMemory state)
        {
            var result = new List<JsonObject>();
            string error = null;
            if (arg == null)
            {
                error = $"The argument {arg} must be a JSON object or array.";
            }
            else if (FunctionUtils.TryParseList(arg, out var array))
            {
                var jsonArray = state.SerializeToNode(array).AsArray();
                foreach (var node in jsonArray)
                {
                    if (node is JsonObject jobj)
                    {
                        result.Add(jobj);
                    }
                    else
                    {
                        error = $"The argument {node} in array must be a JSON object.";
                        break;
                    }
                }
            }
            else
            {
                var node = state.SerializeToNode(arg);
                if (node is JsonObject jobj)
                {
                    result.Add(jobj);
                }
                else
                {
                    error = $"The argument {arg} must be a JSON object or array.";
                }
            }

            return (result, error);
        }
    }
}
