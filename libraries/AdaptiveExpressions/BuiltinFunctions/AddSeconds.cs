// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of seconds to a timestamp.
    /// AddSeconds function takes a timestamp string, an interval integer,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class AddSeconds : TimeTransformEvaluator
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
