// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of hours to a timestamp.
    /// AddHours function takes a timestamp string, an interval integer, an optional format string and an optional locale string.
    /// </summary>
    public class AddHours : TimeTransformEvaluator
    {
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
