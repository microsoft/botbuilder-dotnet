// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the first item from a string or array.
    /// </summary>
    internal class First : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="First"/> class.
        /// </summary>
        public First()
            : base(ExpressionType.First, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            object first = null;
                            if (args[0] is string string0 && string0.Length > 0)
                            {
                                first = string0.First().ToString();
                            }
                            else if (FunctionUtils.TryParseList(args[0], out IList list) && list.Count > 0)
                            {
                                first = FunctionUtils.AccessIndex(list, 0).value;
                            }

                            return first;
                        });
        }
    }
}
