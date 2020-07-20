﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return a string in uppercase format.
    /// If a character in the string doesn't have an uppercase version, that character stays unchanged in the returned string.
    /// </summary>
    public class ToUpper : StringTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToUpper"/> class.
        /// Built-in function ToUpper constructor.
        /// </summary>
        public ToUpper()
            : base(ExpressionType.ToUpper, Function)
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
