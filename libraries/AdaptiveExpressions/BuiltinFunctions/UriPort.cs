// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Memory;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the port value of a unified resource identifier (URI).
    /// </summary>
    public class UriPort : ExpressionEvaluator
    {
        public UriPort()
            : base(ExpressionType.UriPort, Evaluator, ReturnType.Number, FunctionUtils.ValidateUnary)
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
                    (value, error) = EvalUriPort(uri);
                }
                else
                {
                    error = $"{expression} should contain a URI string.";
                }
            }

            return (value, error);
        }

        private static (object, string) EvalUriPort(string uri)
        {
            var (result, error) = FunctionUtils.ParseUri(uri);
            if (error == null)
            {
                try
                {
                    var uriBase = (Uri)result;
                    var port = uriBase.Port;
                    result = Convert.ToInt32(port);
                }
                catch
                {
                    error = "invalid operation, input uri should be an absolute URI";
                }
            }

            return (result, error);
        }
    }
}
