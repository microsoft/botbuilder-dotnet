// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class AddProperty : ExpressionEvaluator
    {
        public AddProperty()
            : base(ExpressionType.AddProperty, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args =>
            {
                var newJobj = (IDictionary<string, JToken>)args[0];
                var prop = args[1].ToString();
                string error = null;
                if (newJobj.ContainsKey(prop))
                {
                    error = $"{prop} already exists";
                }
                else
                {
                    newJobj[prop] = FunctionUtils.ConvertToJToken(args[2]);
                }

                return (newJobj, error);
            });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Object, ReturnType.String, ReturnType.Object);
        }
    }
}
