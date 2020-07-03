// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class ToUpper : StringTransformEvaluator
    {
        public ToUpper(string alias = null)
            : base(alias ?? ExpressionType.ToUpper, Function)
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
                return args[0].ToString().ToUpperInvariant();
            }
        }
    }
}
