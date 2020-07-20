﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Returns the smallest integral value that is greater than or equal to the specified number.
    /// </summary>
    public class Ceiling : NumberTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Ceiling"/> class.
        /// Built-in function Ceiling constructor.
        /// </summary>
        public Ceiling()
                : base(ExpressionType.Ceiling, Function)
        {
        }

        private static object Function(IReadOnlyList<object> args)
        {
            return Math.Ceiling(Convert.ToDouble(args[0], CultureInfo.InvariantCulture));
        }
    }
}
