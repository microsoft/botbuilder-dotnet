// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the string version of a uniform resource identifier (URI) encoded string, effectively decoding the URI-encoded string.
    /// </summary>
    internal class UriComponentToString : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriComponentToString"/> class.
        /// </summary>
        public UriComponentToString()
            : base(ExpressionType.UriComponentToString, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Uri.UnescapeDataString(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
