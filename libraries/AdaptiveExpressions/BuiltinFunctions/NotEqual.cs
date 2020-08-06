// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// return true if the two items are not equal.
    /// </summary>
    internal class NotEqual : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotEqual"/> class.
        /// </summary>
        public NotEqual()
            : base(
                  ExpressionType.NotEqual,
                  (args) => !Function(args),
                  FunctionUtils.ValidateBinary)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            if (args[0] == null || args[1] == null)
            {
                // null will only equals to null
                return args[0] == null && args[1] == null;
            }

            if (FunctionUtils.TryParseList(args[0], out IList l0) && l0.Count == 0 && (FunctionUtils.TryParseList(args[1], out IList l1) && l1.Count == 0))
            {
                return true;
            }

            if (GetPropertyCount(args[0]) == 0 && GetPropertyCount(args[1]) == 0)
            {
                return true;
            }

            if (args[0].IsNumber() && args[0].IsNumber())
            {
                if (Math.Abs(FunctionUtils.CultureInvariantDoubleConvert(args[0]) - FunctionUtils.CultureInvariantDoubleConvert(args[1])) < 0.00000001)
                {
                    return true;
                }
            }

            try
            {
                return args[0] == args[1] || (args[0] != null && args[0].Equals(args[1]));
            }
#pragma warning disable CA1031 // Do not catch general exception types (we return false if the operation fails for whatever reason)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return false;
            }
        }

        private static int GetPropertyCount(object obj)
        {
            if (obj is IDictionary dictionary)
            {
                return dictionary.Count;
            }
            else if (obj is JObject jobj)
            {
                return jobj.Properties().Count();
            }
            else if (!(obj is JValue) && obj.GetType().IsValueType == false && obj.GetType().FullName != "System.String")
            {
                // exclude constant type.
                return obj.GetType().GetProperties().Length;
            }

            return -1;
        }
    }
}
