// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace AdaptiveExpressions.BuiltinFunctions
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
                (args) =>
                {
                    var result = JsonConvert.SerializeObject(args[0], new JsonSerializerSettings { MaxDepth = null });
                    return result;
                });
        }
    }
}
