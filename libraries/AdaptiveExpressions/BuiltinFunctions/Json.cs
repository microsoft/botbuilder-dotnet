// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the JavaScript Object Notation (JSON) type value or object of a string or XML.
    /// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces (by design and we can't change this without breaking binary compat)
    public class Json : ExpressionEvaluator
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        public Json()
            : base(ExpressionType.Json, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            using (var textReader = new StringReader(args[0].ToString()))
                            {
                                using (var jsonReader = new JsonTextReader(textReader) { DateParseHandling = DateParseHandling.None })
                                {
                                    return JToken.ReadFrom(jsonReader);
                                }
                            }
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String);
        }
    }
}
