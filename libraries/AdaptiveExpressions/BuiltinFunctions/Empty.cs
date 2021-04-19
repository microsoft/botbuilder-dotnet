// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether an instance is empty. Return true if the input is empty. Empty means:
    /// 1.input is null or undefined
    /// 2.input is a null or empty string
    /// 3.input is zero size collection
    /// 4.input is an object with no property.
    /// </summary>
    internal class Empty : ComparisonEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Empty"/> class.
        /// </summary>
        public Empty()
            : base(
                  ExpressionType.Empty,
                  Function,
                  FunctionUtils.ValidateUnary,
                  FunctionUtils.VerifyContainerOrNull)
        {
        }

        private static bool Function(IReadOnlyList<object> args)
        {
            return IsEmpty(args[0]);
        }

        private static bool IsEmpty(object instance)
        {
            bool result;
            if (instance == null)
            {
                result = true;
            }
            else if (instance is string string0)
            {
                result = string.IsNullOrEmpty(string0);
            }
            else if (FunctionUtils.TryParseList(instance, out var list))
            {
                result = list.Count == 0;
            }
            else
            {
                result = instance.GetType().GetProperties().Length == 0;
            }

            return result;
        }
    }
}
