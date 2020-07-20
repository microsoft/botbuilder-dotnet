// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a new Guid string.
    /// </summary>
    public class NewGuid : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NewGuid"/> class.
        /// Built-in function NewGuid constructor.
        /// </summary>
        public NewGuid()
            : base(ExpressionType.NewGuid, Evaluator(), ReturnType.String, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => Guid.NewGuid().ToString());
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 0, 0);
        }
    }
}
