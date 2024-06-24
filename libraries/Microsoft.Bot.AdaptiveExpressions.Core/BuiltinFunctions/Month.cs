// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return the month of the specified timestamp.
    /// </summary>
    internal class Month : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Month"/> class.
        /// </summary>
        public Month()
            : base(ExpressionType.Month, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(args => FunctionUtils.NormalizeToDateTime(args[0], dt => (dt.Month, null)));
        }
    }
}
