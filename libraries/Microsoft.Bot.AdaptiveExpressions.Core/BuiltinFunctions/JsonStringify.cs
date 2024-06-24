// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return the json string of a value.
    /// String function takes an object as the argument.
    /// </summary>
    internal class JsonStringify : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonStringify"/> class.
        /// </summary>
        public JsonStringify()
            : base(ExpressionType.JsonStringify, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                (args, state) =>
                {
                    var result = state.JsonSerializeToString(args[0]);
                    return result;
                });
        }
    }
}
