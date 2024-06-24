// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Globalization;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return a random integer from a specified range, which is inclusive only at the starting end.
    /// </summary>
    internal class Rand : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Rand"/> class.
        /// </summary>
        public Rand()
            : base(ExpressionType.Rand, Evaluator, ReturnType.Number, FunctionUtils.ValidateBinaryNumber)
        {
        }

        private static (object, string) Evaluator(Expression expression, IMemory state, Options options)
        {
            object result = null;
            object minValue;
            string error;
            (minValue, error) = expression.Children[0].TryEvaluate(state, options);
            if (error != null)
            {
                return (result, error);
            }

            int minValueInt;
            (minValueInt, error) = FunctionUtils.ParseInt32(minValue);

            if (error != null)
            {
                return (result, error);
            }

            object maxValue;
            (maxValue, error) = expression.Children[1].TryEvaluate(state, options);
            if (error != null)
            {
                return (result, error);
            }

            int maxValueInt;
            (maxValueInt, error) = FunctionUtils.ParseInt32(maxValue);

            if (error != null)
            {
                return (result, error);
            }

            if (minValueInt >= maxValueInt)
            {
                error = $"{minValueInt} is not < {maxValueInt} for rand";
            }
            else
            {
                result = state.RandomNext(minValueInt, maxValueInt);
            }

            return (result, error);
        }
    }
}
