// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the date of a specified timestamp in m/dd/yyyy format.
    /// </summary>
    public class Date : ExpressionEvaluator
    {
        public Date()
            : base(ExpressionType.Date, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => dt.Date.ToString("M/dd/yyyy", CultureInfo.InvariantCulture)));
        }
    }
}
