﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the binary version of a uniform resource identifier (URI) component.
    /// </summary>
    public class UriComponent : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriComponent"/> class.
        /// Built-in function UriComponent constructor.
        /// </summary>
        public UriComponent()
            : base(ExpressionType.UriComponent, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Uri.EscapeDataString(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
