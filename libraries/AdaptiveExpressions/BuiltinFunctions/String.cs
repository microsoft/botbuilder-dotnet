// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the string version of a value.
    /// </summary>
#pragma warning disable CA1716 // Identifiers should not match keywords (by design and can't break binary compat, excluding)
#pragma warning disable CA1720 // Identifier contains type name (by design and can't change this because of backward compat)
    public class String : ExpressionEvaluator
#pragma warning restore CA1720 // Identifier contains type name
#pragma warning restore CA1716 // Identifiers should not match keywords
    {
        public String()
            : base(ExpressionType.String, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => JsonConvert.SerializeObject(args[0]).TrimStart('"').TrimEnd('"'));
        }
    }
}
