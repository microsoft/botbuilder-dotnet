// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Add a number of minutes to a timestamp.
    /// AddMinutes function takes a timestamp string, an interval integer, an optional format string and an optional locale string.
    /// </summary>
    public class AddMinutes : TimeTransformEvaluator
    {
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
