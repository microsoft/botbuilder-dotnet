// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Converts the specified string to title case.
    /// </summary>
    public class TitleCase : StringTransformEvaluator
    {
        public TitleCase()
            : base(ExpressionType.TitleCase, Function)
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
                var ti = CultureInfo.InvariantCulture.TextInfo;
                return ti.ToTitleCase(inputStr);
            }
        }
    }
}
