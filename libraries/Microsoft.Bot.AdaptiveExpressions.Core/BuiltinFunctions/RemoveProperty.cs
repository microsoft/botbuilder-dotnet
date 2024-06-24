// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Nodes;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Remove a property from an object and return the updated object.
    /// </summary>
    internal class RemoveProperty : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoveProperty"/> class.
        /// </summary>
        public RemoveProperty()
            : base(ExpressionType.RemoveProperty, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args =>
            {
                var newJobj = (JsonObject)args[0];
                newJobj.Remove(args[1].ToString());
                return newJobj;
            });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Object, ReturnType.String);
        }
    }
}
