// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check JSON or a JSON string for nodes or values that match a path expression, and return the matching nodes.
    /// </summary>
    internal class JPath : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JPath"/> class.
        /// </summary>
        public JPath()
            : base(ExpressionType.JPath, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => EvalJPath(args[0], args[1].ToString()));
        }

        private static (object, string) EvalJPath(object jsonEntity, string jpath)
        {
            object result = null;
            string error = null;
            object value = null;
            JObject jsonObj = null;
            if (jsonEntity is string jsonStr)
            {
                try
                {
                    jsonObj = JObject.Parse(jsonStr);
                }
#pragma warning disable CA1031 // Do not catch general exception types (we should probably do something about this but ignoring it for now)
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    error = $"{jsonStr} is not a valid JSON string";
                }
            }
            else if (jsonEntity is JObject parsed)
            {
                jsonObj = parsed;
            }
            else
            {
                error = $"{jsonEntity} is not a valid JSON object or a valid JSON string";
            }

            if (error == null)
            {
                try
                {
                    value = jsonObj.SelectTokens(jpath);
                }
#pragma warning disable CA1031 // Do not catch general exception types (we should probably do something about this but ignoring for now)
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    error = $"{jpath} is not a valid path";
                }
            }

            if (error == null)
            {
                if (value is IEnumerable<JToken> products)
                {
                    if (products.Count() == 1)
                    {
                        result = FunctionUtils.ResolveValue(products.ElementAt(0));
                    }
                    else if (products.Count() > 1)
                    {
                        var nodeList = new List<object>();
                        foreach (var item in products)
                        {
                            nodeList.Add(FunctionUtils.ResolveValue(item));
                        }

                        result = nodeList;
                    }
                    else
                    {
                        error = $"there is no matching node for path: {jpath} in the given JSON";
                    }
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Object, ReturnType.String);
        }
    }
}
