// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// The indexing operator ([ ]) selects a single element from a sequence.
    /// Support number index for list or string index for object.
    /// </summary>
    internal class Element : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Element"/> class.
        /// </summary>
        public Element()
            : base(ExpressionType.Element, Evaluator, ReturnType.Object, FunctionUtils.ValidateBinary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error;
            var instance = expression.Children[0];
            var index = expression.Children[1];
            object inst;
            (inst, error) = instance.TryEvaluate(state, options);
            if (error == null)
            {
                object idxValue;
                (idxValue, error) = index.TryEvaluate(state, new Options(options) { NullSubstitution = null });
                if (error == null)
                {
                    if (idxValue.IsInteger())
                    {
                        var idx = 0;
                        (idx, error) = FunctionUtils.ParseInt32(idxValue);
                        if (error == null)
                        {
                            (value, error) = FunctionUtils.AccessIndex(inst, idx);
                        } 
                    }
                    else if (idxValue is string idxStr)
                    {
                        FunctionUtils.TryAccessProperty(inst, idxStr, out value);
                    }
                    else
                    {
                        error = $"Could not coerce {index}<{idxValue?.GetType()}> to an int or string";
                    }
                }
            }

            return (value, error);
        }
    }
}
