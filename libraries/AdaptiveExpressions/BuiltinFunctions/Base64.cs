// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the base64-encoded version of a string or byte array.
    /// </summary>
    internal class Base64 : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Base64"/> class.
        /// </summary>
        public Base64()
            : base(ExpressionType.Base64, Evaluator(), ReturnType.String, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply((args) =>
            {
                byte[] byteArray;
                if (args[0] is byte[] byteArr)
                {
                    byteArray = byteArr;
                }
                else
                {
                    byteArray = System.Text.Encoding.UTF8.GetBytes(args[0].ToString());
                }

                return Convert.ToBase64String(byteArray);
            });
        }
    }
}
