// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Return the last item from a collection.
    /// </summary>
    internal class Last : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Last"/> class.
        /// </summary>
        public Last()
            : base(ExpressionType.Last, Evaluator(), ReturnType.Object, FunctionUtils.ValidateUnary)
        {
        }

        private static EvaluateExpressionDelegate Evaluator()
        {
            return FunctionUtils.Apply(
                        args =>
                        {
                            object last = null;
                            if (args[0] is string string0 && string0.Length > 0)
                            {
                                last = string0.Last().ToString();
                            }
                            else if (FunctionUtils.TryParseList(args[0], out IList list) && list.Count > 0)
                            {
                                last = FunctionUtils.AccessIndex(list, list.Count - 1).value;
                            }

                            return last;
                        });
        }
    }
}
