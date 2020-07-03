// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Power : MultivariateNumericEvaluator
    {
        public Power(string alias = null)
            : base(alias ?? ExpressionType.Power, Evaluator, FunctionUtils.VerifyNumericListOrNumber)
        {
        }

        private static object Evaluator(IReadOnlyList<object> args)
        {
            return Math.Pow(FunctionUtils.CultureInvariantDoubleConvert(args[0]), FunctionUtils.CultureInvariantDoubleConvert(args[1]));
        }
    }
}
