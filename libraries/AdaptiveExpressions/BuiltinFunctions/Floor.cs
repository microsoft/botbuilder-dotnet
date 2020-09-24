// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Returns the largest integer less than or equal to the specified number.
    /// </summary>
    internal class Floor : NumberTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Floor"/> class.
        /// </summary>
        public Floor()
            : base(ExpressionType.Floor, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            return Math.Floor(Convert.ToDouble(args[0], CultureInfo.InvariantCulture));
        }
    }
}
