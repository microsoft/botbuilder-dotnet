// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the ticks property value of a specified timestamp. A tick is 100-nanosecond interval.
    /// </summary>
    internal class Ticks : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ticks"/> class.
        /// </summary>
        public Ticks()
            : base(ExpressionType.Ticks, Evaluator, ReturnType.Number, Validator)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                (value, error) = FunctionUtils.TicksWithError(args[0]);
            }

            return (value, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 1, 1, ReturnType.String);
        }
    }
}
