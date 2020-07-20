﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of days to a timestamp.
    /// </summary>
    public class AddDays : TimeTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddDays"/> class.
        /// Built-in function AddDays constructor.
        /// </summary>
        public AddDays()
                : base(ExpressionType.AddDays, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddDays(interval);
        }
    }
}
