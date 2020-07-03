// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Unique : ExpressionEvaluator
    {
        public Unique(string alias = null)
            : base(alias ?? ExpressionType.Unique, Evaluator(), ReturnType.Array, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            return ((IEnumerable<object>)args[0]).Distinct().ToList();
                        }, FunctionUtils.VerifyList);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array);
        }
    }
}
