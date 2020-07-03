// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsObject : ExpressionEvaluator
    {
        public IsObject(string alias = null)
            : base(alias ?? ExpressionType.IsObject, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => args[0] != null && !(args[0] is JValue) && args[0].GetType().IsValueType == false && args[0].GetType() != typeof(string));
        }
    }
}
