// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a data uniform resource identifier (URI) of a string.
    /// </summary>
    internal class DataUri : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataUri"/> class.
        /// </summary>
        public DataUri()
            : base(ExpressionType.DataUri, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => "data:text/plain;charset=utf-8;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(args[0].ToString())), FunctionUtils.VerifyString);
        }
    }
}
