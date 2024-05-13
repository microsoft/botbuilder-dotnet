// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the binary version of a string.
    /// </summary>
#pragma warning disable CA1724 // Type names should not match namespaces (by design and we can't change this without breaking binary compat)
    internal class Binary : ExpressionEvaluator
#pragma warning restore CA1724 // Type names should not match namespaces
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Binary"/> class.
        /// </summary>
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
