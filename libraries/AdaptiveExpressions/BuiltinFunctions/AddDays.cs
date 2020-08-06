// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of days to a timestamp.
    /// AddDays function takes a timestamp string, an interval integer,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class AddDays : TimeTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddDays"/> class.
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
