// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of minutes to a timestamp.
    /// </summary>
    public class AddMinutes : TimeTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddMinutes"/> class.
        /// Built-in function AddMinutes constructor.
        /// </summary>
        public AddMinutes()
                : base(ExpressionType.AddMinutes, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddMinutes(interval);
        }
    }
}
