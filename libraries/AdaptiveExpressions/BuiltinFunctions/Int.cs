// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the integer version of a string.
    /// </summary>
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
    internal class Int : ExpressionEvaluator
#pragma warning restore CA1720 // Identifier contains type name
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Int"/> class.
        /// </summary>
        public Int()
            : base(ExpressionType.Int, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Convert.ToInt64(args[0], CultureInfo.InvariantCulture));
        }
    }
}
