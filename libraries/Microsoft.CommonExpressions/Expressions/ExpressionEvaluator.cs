// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Expressions
{
    public delegate void ValidateExpressionDelegate(Expression expression);
    public delegate (object value, string error) EvaluateExpressionDelegate(Expression expression, object state);

    public class ExpressionEvaluator
    {
        public ExpressionEvaluator(EvaluateExpressionDelegate evaluator,
            ExpressionReturnType returnType = ExpressionReturnType.Object,
            ValidateExpressionDelegate validator = null)
        {
            _evaluator = evaluator;
            ReturnType = returnType;
            _validator = validator ?? new ValidateExpressionDelegate((expr) => { });
        }

        private ValidateExpressionDelegate _validator;
        private EvaluateExpressionDelegate _evaluator;

        public (object value, string error) TryEvaluate(Expression expression, object state)
            => _evaluator(expression, state);

        public void ValidateExpression(Expression expression)
            => _validator(expression);

        public ExpressionReturnType ReturnType { get; }
    }
}
