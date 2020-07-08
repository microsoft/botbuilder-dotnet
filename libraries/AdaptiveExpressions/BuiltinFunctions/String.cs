// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the string version of a value.
    /// </summary>
    public class String : ExpressionEvaluator
    {
        public String()
            : base(ExpressionType.String, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => JsonConvert.SerializeObject(args[0]).TrimStart('"').TrimEnd('"'));
        }
    }
}
