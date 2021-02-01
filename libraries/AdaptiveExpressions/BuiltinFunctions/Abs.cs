// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Returns the absolute value of the specified number.
    /// </summary>
    internal class Abs : NumberTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Abs"/> class.
        /// </summary>
        public Abs()
                : base(ExpressionType.Abs, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            return Math.Abs(Convert.ToDouble(args[0], CultureInfo.InvariantCulture));
        }
    }
}
