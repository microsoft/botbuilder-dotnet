// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Convert the string version of a floating-point number to a floating-point number. You can use this function only when passing custom parameters to an app, such as a logic app.
    /// </summary>
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
    internal class Float : ExpressionEvaluator
#pragma warning restore CA1720 // Identifier contains type name
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Float"/> class.
        /// </summary>
        public Float()
            : base(ExpressionType.Float, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.CultureInvariantDoubleConvert(args[0]));
        }
    }
}
