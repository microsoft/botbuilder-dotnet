// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is an integer number. Due to the alignment between C# and JavaScript, a number with an zero residue of its modulo 1 will be treated as an integer number.
    /// </summary>
    internal class IsInteger : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsInteger"/> class.
        /// </summary>
        public IsInteger()
            : base(ExpressionType.IsInteger, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Extensions.IsNumber(args[0]) && FunctionUtils.CultureInvariantDoubleConvert(args[0]) % 1 == 0);
        }
    }
}
