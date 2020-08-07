// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of minutes to a timestamp.
    /// AddMinutes function takes a timestamp string, an interval integer,
    /// an optional format string whose default value "yyyy-MM-ddTHH:mm:ss.fffZ"
    /// and an optional locale string whose default value is Thread.CurrentThread.CurrentCulture.Name.
    /// </summary>
    internal class AddMinutes : TimeTransformEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddMinutes"/> class.
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
