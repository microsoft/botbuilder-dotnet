// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class CreateArray : ExpressionEvaluator
    {
        public CreateArray()
            : base(ExpressionType.CreateArray, Evaluator(), ReturnType.Array)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => new List<object>(args));
        }
    }
}
