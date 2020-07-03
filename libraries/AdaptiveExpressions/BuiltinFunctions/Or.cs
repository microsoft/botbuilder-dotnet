// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Or : ExpressionEvaluator
    {
        public Or(string alias = null)
            : base(alias ?? ExpressionType.Or, EvalOr, ReturnType.Boolean, FunctionUtils.ValidateAtLeastOne)
        {
        }

        private static (object value, string error) EvalOr(Expression expression, IMemory state, Options options)
        {
            object result = false;
            string error = null;
            foreach (var child in expression.Children)
            {
                (result, error) = child.TryEvaluate(state, new Options(options) { NullSubstitution = null });
                if (error == null)
                {
                    if (FunctionUtils.IsLogicTrue(result))
                    {
                        result = true;
                        break;
                    }
                }
                else
                {
                    // Interpret error as false and swallow errors
                    error = null;
                }
            }

            return (result, error);
        }
    }
}
