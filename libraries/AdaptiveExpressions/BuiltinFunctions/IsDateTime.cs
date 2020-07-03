// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class IsDateTime : ExpressionEvaluator
    {
        public IsDateTime(string alias = null)
            : base(alias ?? ExpressionType.IsDateTime, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            object value = null;
                            string error = null;
                            (value, error) = FunctionUtils.NormalizeToDateTime(args[0]);
                            if (error == null)
                            {
                                return true;
                            }

                            return false;
                        });
        }
    }
}
