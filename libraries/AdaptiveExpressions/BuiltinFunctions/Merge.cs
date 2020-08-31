// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Merge two object(json) into one object(json).
    /// </summary>
    internal class Merge : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Merge"/> class.
        /// </summary>
        public Merge()
            : base(ExpressionType.Merge, Evaluator(), ReturnType.Object, Validator)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplySequenceWithError(args =>
            {
                object result = null;
                string error = null;
                var arg0 = FunctionUtils.ConvertToJToken(args[0]);
                var arg1 = FunctionUtils.ConvertToJToken(args[1]);
                if (arg0 is JObject && arg1 is JObject)
                {
                    (arg0 as JObject).Merge(arg1 as JObject, new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Replace
                    });

                    result = arg0;
                }
                else
                {
                    error = $"The arguments {args[0]} and {args[1]} must be a JSON objects.";
                }

                return (result, error);
            });
        }

        private static void Validator(Expression expression)
        {
            FunctionUtils.ValidateArityAndAnyType(expression, 2, int.MaxValue);
        }
    }
}
