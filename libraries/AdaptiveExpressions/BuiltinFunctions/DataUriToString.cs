// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the string version of a data uniform resource identifier (URI).
    /// </summary>
    internal class DataUriToString : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataUriToString"/> class.
        /// </summary>
        public DataUriToString()
            : base(ExpressionType.DataUriToString, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(args[0].ToString().Substring(args[0].ToString().IndexOf(",", StringComparison.Ordinal) + 1))), FunctionUtils.VerifyString);
        }
    }
}
