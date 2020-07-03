// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the year of the specified timestamp.
    /// </summary>
    public class Year : ExpressionEvaluator
    {
        public Year()
            : base(ExpressionType.Year, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.Year));
        }
    }
}
