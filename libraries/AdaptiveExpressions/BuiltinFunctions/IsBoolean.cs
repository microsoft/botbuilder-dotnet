﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is a Boolean.
    /// </summary>
    public class IsBoolean : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsBoolean"/> class.
        /// Built-in function IsBoolean constructor.
        /// </summary>
        public IsBoolean()
            : base(ExpressionType.IsBoolean, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => args[0] is bool);
        }
    }
}
