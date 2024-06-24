// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Bot.AdaptiveExpressions.Core.Memory;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
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

        private static bool Function(IReadOnlyList<object> args, IMemory state)
        {
            return IsEmpty(args[0]);
        }

        [UnconditionalSuppressMessage("Trimming", "IL2075:'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicProperties' in call to 'System.Type.GetProperties()'. The return value of method 'System.Object.GetType()' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target location it is assigned to.", Justification = "AOT aware callers will not go through reflection path")]
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
