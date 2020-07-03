using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IndicesAndValues : ExpressionEvaluator
    {
        public IndicesAndValues(string alias = null)
            : base(alias ?? ExpressionType.IndicesAndValues, EvalIndicesAndValues, ReturnType.Array, FunctionUtils.ValidateUnary)
        {
        }

        private static (object, string) EvalIndicesAndValues(Expression expression, object state, Options options)
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
