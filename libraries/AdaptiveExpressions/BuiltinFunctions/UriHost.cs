// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the host value of a unified resource identifier (URI).
    /// </summary>
    internal class UriHost : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriHost"/> class.
        /// </summary>
        public UriHost()
            : base(ExpressionType.UriHost, Evaluator, ReturnType.String, FunctionUtils.ValidateUnary)
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
                    (value, error) = EvalUriHost(uri);
                }
                else
                {
                    error = $"{expression} should contain a URI string.";
                }
            }

            return (value, error);
        }

        private static (object, string) EvalUriHost(string uri)
        {
            var (result, error) = FunctionUtils.ParseUri(uri);

            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    var host = uriBase.Host;
                    result = host.ToString();
                }
#pragma warning disable CA1031 // Do not catch general exception types (we are capturing the exception and returning a generic error for all failures)
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
