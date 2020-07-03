// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the binary version of a string.
    /// </summary>
    public class Binary : ExpressionEvaluator
    {
        public Binary()
            : base(ExpressionType.Binary, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.ToBinary(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
