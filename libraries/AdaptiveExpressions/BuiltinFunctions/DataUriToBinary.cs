// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the binary version of a data uniform resource identifier (URI).
    /// </summary>
    internal class DataUriToBinary : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataUriToBinary"/> class.
        /// </summary>
        public DataUriToBinary()
            : base(ExpressionType.DataUriToBinary, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(args => FunctionUtils.ToBinary(args[0].ToString()), FunctionUtils.VerifyString);
        }
    }
}
