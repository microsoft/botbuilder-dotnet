// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of seconds to a timestamp.
    /// </summary>
    public class AddSeconds : TimeTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddSeconds"/> class.
        /// </summary>
        public AddSeconds()
                : base(ExpressionType.AddSeconds, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddSeconds(interval);
        }
    }
}
