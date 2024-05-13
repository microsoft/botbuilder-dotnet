// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of hours to a timestamp.
    /// AddHours function takes a timestamp string, an interval integer,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class AddHours : TimeTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddHours"/> class.
        /// </summary>
        public AddHours()
                : base(ExpressionType.AddHours, Function)
        {
        }

        private static DateTime Function(DateTime time, int interval)
        {
            return time.AddHours(interval);
        }
    }
}
