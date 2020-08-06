// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the path and query value of a unified resource identifier (URI).
    /// </summary>
    internal class UriPathAndQuery : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UriPathAndQuery"/> class.
        /// </summary>
        public UriPathAndQuery()
            : base(ExpressionType.UriPathAndQuery, Evaluator, ReturnType.String, FunctionUtils.ValidateUnary)
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
                    (value, error) = EvalUriPathAndQuery(uri);
                }
                else
                {
                    error = $"{expression} should contain a URI string.";
                }
            }

            return (value, error);
        }

        private static (object, string) EvalUriPathAndQuery(string uri)
        {
            object result = null;
            string error = null;
            Uri uriBase = null;
            try
            {
                uriBase = new Uri(uri);
            }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return generic error)
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                error = "illegal URI string";
            }

            if (error == null)
            {
                try
                {
                    var pathAndQuery = uriBase.PathAndQuery;
                    result = pathAndQuery.ToString();
                }
#pragma warning disable CA1031 // Do not catch general exception types (capture any exception and return generic error)
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
