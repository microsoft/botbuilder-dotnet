// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return an integer array that starts from a specified integer.
    /// </summary>
    internal class Range : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> class.
        /// </summary>
        public Range()
            : base(ExpressionType.Range, Evaluator(), ReturnType.Array, FunctionUtils.ValidateBinaryNumber)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.ApplyWithError(
                        args =>
                        {
                            string error = null;
                            IList result = null;
                            var count = 0;                
                            (count, error) = FunctionUtils.ParseInt32(args[1]);
                            if (error == null)
                            {
                                if (count <= 0)
                                {
                                    error = $"The second parameter {args[1]} should be more than zero";
                                }
                                else
                                {
                                    var start = 0;
                                    (start, error) = FunctionUtils.ParseInt32(args[0]);
                                    if (error == null)
                                    {
                                        result = Enumerable.Range(start, count).ToList();
                                    }
                                }
                            }                         

                            return (result, error);
                        },
                        FunctionUtils.VerifyInteger);
        }
    }
}
