// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is a floating-point number.
    /// Due to the alignment between C#and JavaScript, a number with an non-zero residue of its modulo 1 will be treated as a floating-point number.
    /// </summary>
    internal class IsFloat : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsFloat"/> class.
        /// </summary>
        public IsFloat()
            : base(ExpressionType.IsFloat, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Extensions.IsNumber(args[0]) && FunctionUtils.CultureInvariantDoubleConvert(args[0]) % 1 != 0);
        }
    }
}
