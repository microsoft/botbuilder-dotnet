// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Coalesce : ExpressionEvaluator
    {
        public Coalesce(string alias = null)
            : base(alias ?? ExpressionType.Coalesce, Evaluator(), ReturnType.Object, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => EvalCoalesce(args.ToArray()));
        }

        private static object EvalCoalesce(object[] objectList)
        {
            foreach (var obj in objectList)
            {
                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }
    }
}
