// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
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
                args =>
                {
                    var result = new JObject();

                    foreach (var arg in args)
                    {
                        var (list, itemError) = ParseToObjectList(arg);

                        if (itemError != null)
                        {
                            return (null, itemError);
                        }

                        foreach (var item in list)
                        {
                            result.Merge(item, new JsonMergeSettings
                            {
                                MergeArrayHandling = MergeArrayHandling.Replace
                            });
                        }
                    }

                    return (result, null);
                });
        }

        private static (List<JObject>, string) ParseToObjectList(object arg)
        {
            var result = new List<JObject>();
            string error = null;
            if (arg == null)
            {
                error = $"The argument {arg} must be a JSON object or array.";
            }
            else if (FunctionUtils.TryParseList(arg, out var array))
            {
                var jarray = JArray.FromObject(array);
                foreach (var jtoken in jarray)
                {
                    if (jtoken is JObject jobj)
                    {
                        result.Add(jobj);
                    }
                    else
                    {
                        error = $"The argument {jtoken} in array must be a JSON object.";
                        break;
                    }
                }
            }
            else
            {
                var jtoken = FunctionUtils.ConvertToJToken(arg);
                if (jtoken is JObject jobj)
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
