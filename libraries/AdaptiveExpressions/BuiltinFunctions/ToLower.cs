// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a string in lowercase format.
    /// If a character in the string doesn't have a lowercase version, that character stays unchanged in the returned string.
    /// </summary>
    public class ToLower : StringTransformEvaluator
    {
        public ToLower()
            : base(ExpressionType.ToLower, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            if (args[0] == null)
            {
                return string.Empty;
            }
            else
            {
                return args[0].ToString().ToLowerInvariant();
            }
        }
    }
}
