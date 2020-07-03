// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class DataUriToString : ExpressionEvaluator
    {
        public DataUriToString()
            : base(ExpressionType.DataUriToString, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString().Substring(args[0].ToString().IndexOf(",") + 1))), FunctionUtils.VerifyString);
        }
    }
}
