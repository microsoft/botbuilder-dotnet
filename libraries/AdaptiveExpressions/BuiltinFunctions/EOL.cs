// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// End of line. Return \r\n in windows, and \n in unix.
    /// </summary>
    internal class EOL : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EOL"/> class.
        /// </summary>
        public EOL()
            : base(ExpressionType.EOL, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Environment.NewLine);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 0, 0);
        }
    }
}
