﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using Microsoft.AdaptiveExpressions.Core.Memory;

namespace Microsoft.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return the starting position or index value of a substring. This function is case-insensitive, and indexes start with the number 0.
    /// </summary>
    internal class IndexOf : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexOf"/> class.
        /// </summary>
        public IndexOf()
            : base(ExpressionType.IndexOf, Evaluator, ReturnType.Number, Validator)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object result = -1;
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0] is string || args[0] == null)
                {
                    if (args[1] is string || args[1] == null)
                    {
                        result = FunctionUtils.ParseStringOrNull(args[0]).IndexOf(FunctionUtils.ParseStringOrNull(args[1]), StringComparison.Ordinal);
                    }
                    else
                    {
                        error = $"Can only look for indexof string in {expression}";
                    }
                }
                else if (FunctionUtils.TryParseList(args[0], out IList list))
                {
                    result = FunctionUtils.ResolveListValue(list).IndexOf(args[1]);
                }
                else
                {
                    error = $"{expression} works only on string or list.";
                }
            }

            return (result, error);
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.Array | ReturnType.String, ReturnType.Object);
        }
    }
}
