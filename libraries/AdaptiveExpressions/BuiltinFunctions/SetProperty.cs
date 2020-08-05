// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Set the value of an object's property and return the updated object. 
    /// </summary>
    internal class SetProperty : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetProperty"/> class.
        /// </summary>
        public SetProperty()
            : base(ExpressionType.SetProperty, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args =>
            {
                var newJobj = (IDictionary<string, JToken>)args[0];
                newJobj[args[1].ToString()] = FunctionUtils.ConvertToJToken(args[2]);
                return newJobj;
            });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Object, ReturnType.String, ReturnType.Object);
        }
    }
}
