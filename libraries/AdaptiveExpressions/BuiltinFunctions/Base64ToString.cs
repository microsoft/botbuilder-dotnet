// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the string version of a base64-encoded string,
    /// effectively decoding the base64 string.
    /// </summary>
    public class Base64ToString : ExpressionEvaluator
    {
        public Base64ToString()
            : base(ExpressionType.Base64ToString, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString())), FunctionUtils.VerifyString);
        }
    }
}
