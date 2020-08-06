// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a property and its value, or name-value pair, to a JSON object, and return the updated object.
    /// If the object already exists at runtime the function throws an error.
    /// </summary>
    internal class AddProperty : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddProperty"/> class.
        /// </summary>
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
