// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Converts the specified string to sentence case.
    /// </summary>
    public class SentenceCase : StringTransformEvaluator
    {
        public SentenceCase()
            : base(ExpressionType.SentenceCase, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            var inputStr = (string)args[0];
            if (string.IsNullOrEmpty(inputStr))
            {
                return string.Empty;
            }
            else
            {
                return inputStr.Substring(0, 1).ToUpperInvariant() + inputStr.Substring(1).ToLowerInvariant();
            }
        }
    }
}
