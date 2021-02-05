// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace AdaptiveExpressions.BuiltinFunctions
{
    /// <summary>
    /// Wrap string interpolation to get real value.
    /// For example: stringOrValue('${1}'), would get number 1
    /// stringOrValue('${1} item'), would get string "1 item".
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

            if (!(stringInput is string))
            {
                error = "Parameter should be a string.";
            }

            if (error == null)
            {
                var expr = Expression.Parse("`" + stringInput + "`");
                var firstChildren = expr.Children[0];
                var secondChildren = expr.Children[1];

                // If the Expression follows this format:
                // concat('', childExpression)
                // return the childExpression result directly.
                if ((firstChildren is Constant child)
                    && (child.Value.ToString().Length == 0)
                    && !(secondChildren is Constant)
                    && expr.Children.Length == 2)
                {
                    (result, error) = secondChildren.TryEvaluate(state, options);
                }
                else
                {
                    (result, error) = expr.TryEvaluate(state, options);
                }
            }

            return (result, error);
        }
    }
}
