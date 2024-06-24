// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.AdaptiveExpressions.Core.BuiltinFunctions
{
    /// <summary>
    /// Return an array from multiple inputs.
    /// </summary>
    internal class CreateArray : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateArray"/> class.
        /// </summary>
        public CreateArray()
            : base(ExpressionType.CreateArray, Evaluator(), ReturnType.Array)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => new List<object>(args));
        }
    }
}
