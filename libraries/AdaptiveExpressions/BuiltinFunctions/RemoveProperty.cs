// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class RemoveProperty : ExpressionEvaluator
    {
        public RemoveProperty(string alias = null)
            : base(alias ?? ExpressionType.RemoveProperty, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args =>
            {
                var newJobj = (JObject)args[0];
                newJobj.Property(args[1].ToString()).Remove();
                return newJobj;
            });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Object, ReturnType.String);
        }
    }
}
