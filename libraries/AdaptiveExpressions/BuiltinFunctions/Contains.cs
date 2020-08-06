// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Check whether a collection has a specific item. Return true if the item is found,
    /// or return false if not found.
    /// This function is case-sensitive.
    /// </summary>
    internal class Contains : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Contains"/> class.
        /// </summary>
        public Contains()
            : base(ExpressionType.Contains, Evaluator, ReturnType.Boolean, FunctionUtils.ValidateBinary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            var found = false;
            var (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0] is string string0 && args[1] is string string1)
                {
                    found = string0.Contains(string1);
                }
                else if (FunctionUtils.TryParseList(args[0], out IList ilist))
                {
                    // list to find a value
                    var operands = FunctionUtils.ResolveListValue(ilist);
                    found = operands.Contains(args[1]);
                }
                else if (args[1] is string string2)
                {
                    found = FunctionUtils.TryAccessProperty((object)args[0], string2, out var _);
                }
            }

            return (found, null);
        }
    }
}
