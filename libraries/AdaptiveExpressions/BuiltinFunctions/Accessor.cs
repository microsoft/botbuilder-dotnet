// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Used to access the variable value corresponding to the path.
    /// </summary>
    internal class Accessor : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Accessor"/> class.
        /// </summary>
        public Accessor()
            : base(ExpressionType.Accessor, Evaluator, ReturnType.Object, Validator)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            var (path, left, error) = FunctionUtils.TryAccumulatePath(expression, state, options);

            if (error != null)
            {
                return (null, error);
            }

            if (left == null)
            {
                // fully converted to path, so we just delegate to memory scope
                return FunctionUtils.WrapGetValue(state, path, options);
            }
            else
            {
                // stop at somewhere, so we figure out what's left
                var (newScope, err) = left.TryEvaluate(state, options);
                if (err != null)
                {
                    return (null, err);
                }

                return FunctionUtils.WrapGetValue(MemoryFactory.Create(newScope), path, options);
            }
        }

        private static void Validator(Expression expression)
        {
            var children = expression.Children;
            if (children.Length == 0
                || !(children[0] is Constant cnst)
                || cnst.ReturnType != ReturnType.String)
            {
                throw new Exception($"{expression} must have a string as first argument.");
            }

            if (children.Length > 2)
            {
                throw new Exception($"{expression} has more than 2 children.");
            }

            if (children.Length == 2 && (children[1].ReturnType & ReturnType.Object) == 0)
            {
                throw new Exception($"{expression} must have an object as its second argument.");
            }
        }
    }
}
