// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    public class Element : ExpressionEvaluator
    {
        public Element(string alias = null)
            : base(alias ?? ExpressionType.Element, ExtractElement, ReturnType.Object, FunctionUtils.ValidateBinary)
        {
        }

        private static (object value, string error) ExtractElement(Expression expression, IMemory state, Options options)
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
                        var idx = Convert.ToInt32(idxValue);
                        (value, error) = FunctionUtils.AccessIndex(inst, idx);
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
