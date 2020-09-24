// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is a complex object or return false if it is a primitive object.
    /// Primitive objects include strings, numbers, and Booleans;
    /// complex types, like classes, contain properties.
    /// </summary>
    internal class IsObject : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsObject"/> class.
        /// </summary>
        public IsObject()
            : base(ExpressionType.IsObject, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => args[0] != null && !(args[0] is JValue) && args[0].GetType().IsValueType == false && args[0].GetType() != typeof(string));
        }
    }
}
