// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the first non-null value from one or more parameters.
    /// Empty strings, empty arrays, and empty objects are not null.
    /// </summary>
    internal class Coalesce : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Coalesce"/> class.
        /// </summary>
        public Coalesce()
            : base(ExpressionType.Coalesce, Evaluator(), ReturnType.Object, FunctionUtils.ValidateAtLeastOne)
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
