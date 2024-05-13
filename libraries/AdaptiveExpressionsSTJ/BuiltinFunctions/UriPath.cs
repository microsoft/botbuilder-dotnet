// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the path value of a unified resource identifier (URI).
    /// </summary>
    internal class UriPath : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriPath"/> class.
        /// </summary>
        public UriPath()
            : base(ExpressionType.UriPath, Evaluator, ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static (object value, string error) Evaluator(Expression expression, IMemory state, Options options)
        {
            object value = null;
            string error = null;
            IReadOnlyList<object> args;
            (args, error) = FunctionUtils.EvaluateChildren(expression, state, options);
            if (error == null)
            {
                if (args[0] is string uri)
                {
                    (value, error) = EvalUriPath(uri);
                }
                else
                {
                    error = $"{expression} should contain a URI string.";
                }
            }

            return (value, error);
        }

        private static (object, string) EvalUriPath(string uri)
        {
            var (result, error) = FunctionUtils.ParseUri(uri);

            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    result = uriBase.AbsolutePath.ToString();
                }
#pragma warning disable CA1031 // Do not catch general exception types (return generic error if the operation fails for whatever reason)
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }
    }
}
