// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.RegularExpressions;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the number of words in a string.
    /// </summary>
    internal class CountWord : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CountWord"/> class.
        /// </summary>
        public CountWord()
            : base(ExpressionType.CountWord, Evaluator(), ReturnType.Number, FunctionUtils.ValidateUnaryString)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            if (args[0] is string)
                            {
                                return Regex.Split(args[0].ToString().Trim(), @"\s{1,}").Length;
                            }
                            else
                            {
                                return 0;
                            }
                        }, FunctionUtils.VerifyStringOrNull);
        }
    }
}
