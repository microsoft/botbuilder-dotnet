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
            : base(ExpressionType.Where, Evaluator, ReturnType.Array, FunctionUtils.ValidateForeach)
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
                var isInstanceList = false;
                IList list = null;
                if (FunctionUtils.TryParseList(instance, out IList ilist))
                {
                    isInstanceList = true;
                    list = ilist;
                }
                else if (instance is JObject jobj)
                {
                    list = FunctionUtils.Object2KVPairList(jobj);
                }
                else if (FunctionUtils.ConvertToJToken(instance) is JObject jobject)
                {
                    list = FunctionUtils.Object2KVPairList(jobject);
                }
                else
                {
                    error = $"{expression.Children[0]} is not a collection or structure object to run foreach";
                }

                if (error == null)
                {
                    var iteratorName = (string)(expression.Children[1].Children[0] as Constant).Value;
                    var stackedMemory = StackedMemory.Wrap(state);
                    result = new List<object>();
                    for (var idx = 0; idx < list.Count; idx++)
                    {
                        var local = new Dictionary<string, object>
                        {
                            { iteratorName, FunctionUtils.AccessIndex(list, idx).value },
                        };

                        // the local iterator is pushed as one memory layer in the memory stack
                        stackedMemory.Push(new SimpleObjectMemory(local));
                        var (r, e) = expression.Children[2].TryEvaluate(stackedMemory, new Options(options) { NullSubstitution = null });
                        stackedMemory.Pop();

                        if (FunctionUtils.IsLogicTrue(r) && e == null)
                        {
                            // add if only if it evaluates to true
                            ((List<object>)result).Add(local[iteratorName]);
                        }
                    }

                    if (!isInstanceList)
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
