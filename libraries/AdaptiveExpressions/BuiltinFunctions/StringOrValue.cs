// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// TODO.
    /// </summary>
    internal class StringOrValue : ExpressionEvaluator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringOrValue"/> class.
        /// </summary>
        public StringOrValue()
            : base(ExpressionType.StringOrValue, Evaluator, ReturnType.Object, FunctionUtils.ValidateUnaryString)
        {
        }

        private static (object, string) Evaluator(Expression expression, object state, Options options)
        {
            object result = null;
            string error;
            object stringInput;
            (stringInput, error) = expression.Children[0].TryEvaluate(state, options);

            // TODO
            // verify string
            if (error == null)
            {
                var expr = Expression.Parse("`" + stringInput + "`");
                var firstChildren = expr.Children[0];
                var secondChildren = expr.Children[1];
                if ((firstChildren is Constant child) && (child.Value.ToString().Length == 0) && !(secondChildren is Constant))
                {
                    
                }
            }

            return (result, error);
        }
    }
}
