// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace AdaptiveExpressions
{
    /// <summary>
    /// Extension methods for detecting or value testing of various types.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Test an object to see if it is a numeric type.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>True if numeric type.</returns>
        public static bool IsNumber(this object value)
            => value is sbyte
            || value is byte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal;

        /// <summary>
        /// Test an object to see if it is an integer type.
        /// </summary>
        /// <param name="value">Value to check.</param>
        /// <returns>True if numeric type.</returns>
        public static bool IsInteger(this object value)
            => value is sbyte
            || value is byte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong;

        /// <summary>
        /// Get the type names of current return type contains.
        /// </summary>
        /// <param name="returntype">enum of ReturnType.</param>
        /// <returns>The type names of current return type contains.</returns>
        public static List<string> Names(this ReturnType returntype)
        {
            var result = new List<string>();

            foreach (ReturnType type in Enum.GetValues(typeof(ReturnType)))
            {
                if ((returntype & type) != 0)
                {
                    result.Add(Enum.GetName(typeof(ReturnType), type));
                }
            }

            return result;
        }
    }
}
