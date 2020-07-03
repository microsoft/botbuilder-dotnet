// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class AddMinutes : TimeTransformEvaluator
    {
        public AddMinutes()
                : base(ExpressionType.AddMinutes, Evaluator)
        {
        }

        private static DateTime Evaluator(DateTime time, int interval)
        {
            return time.AddMinutes(interval);
        }
    }
}
