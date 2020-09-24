// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return true if a given input is a UTC ISO format (YYYY-MM-DDTHH:mm:ss.fffZ) timestamp string.
    /// </summary>
    internal class IsDateTime : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IsDateTime"/> class.
        /// </summary>
        public IsDateTime()
            : base(ExpressionType.IsDateTime, Evaluator(), ReturnType.Boolean, FunctionUtils.ValidateUnary)
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
