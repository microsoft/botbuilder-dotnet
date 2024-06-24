// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return the JavaScript Object Notation (JSON) type value or object of a string or XML.
    /// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces (by design and we can't change this without breaking binary compat)
    internal class Json : ExpressionEvaluator
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Json"/> class.
        /// </summary>
        public Json()
            : base(ExpressionType.Json, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            object result = null;
                            string error = null;
                            try
                            {
                                result = JsonObject.Parse(args[0].ToString());
                            }
                            catch (JsonException err)
                            {
                                error = err.Message;
                            }

                            return (result, error);
                        });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateOrder(expression, null, ReturnType.String);
        }
    }
}
