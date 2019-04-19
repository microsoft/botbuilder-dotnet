// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Expressions
{
    /// <summary>
    /// Delegate for doing static validation on an expression.
    /// </summary>
    /// <param name="expression">Expression to check.</param>
    public delegate void ValidateExpressionDelegate(Expression expression);

    /// <summary>
    /// Delegate to evaluate an expression.
    /// </summary>
    /// <param name="expression">Expression to evaluate.</param>
    /// <param name="state">Global state information.</param>
    /// <returns>Value and error string that is non-null if there is an error.</returns>
    public delegate (object value, string error) EvaluateExpressionDelegate(Expression expression, object state);

    /// <summary>
    /// Delegate to lookup function information from the type.
    /// </summary>
    /// <param name="type">Name to lookup, usually from <see cref="ExpressionType"/></param>
    /// <returns>Expression evaluation information.</returns>
    public delegate ExpressionEvaluator EvaluatorLookup(string type);

    /// <summary>
    /// Information on how to evaluate an expression.
    /// </summary>
    public class ExpressionEvaluator
    {
        /// <summary>
        /// Constructor for expression information.
        /// </summary>
        /// <param name="type">Expression type.</param>
        /// <param name="evaluator">Delegate to evaluate an expression.</param>
        /// <param name="returnType">Type expected from evaluation.</param>
        /// <param name="validator">Static validation of expression.</param>
        public ExpressionEvaluator(string type, EvaluateExpressionDelegate evaluator,
            ReturnType returnType = ReturnType.Object,
            ValidateExpressionDelegate validator = null)
        {
            Type = type;
            _evaluator = evaluator;
            ReturnType = returnType;
            _validator = validator ?? new ValidateExpressionDelegate((expr) => { });
        }

        private ValidateExpressionDelegate _validator;
        private EvaluateExpressionDelegate _evaluator;

        /// <summary>
        /// Expression type for evaluator.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Evaluate an expression.
        /// </summary>
        /// <param name="expression">Expression to evaluate.</param>
        /// <param name="state">Global state information.</param>
        /// <returns>Value and error string that is non-null if there is an error.</returns>
        public (object value, string error) TryEvaluate(Expression expression, object state)
            => _evaluator(expression, state);

        /// <summary>
        /// Validate an expression.
        /// </summary>
        /// <param name="expression">Expression to validate.</param>
        public void ValidateExpression(Expression expression)
            => _validator(expression);

        /// <summary>
        /// Type expected by evaluating the expression.
        /// </summary>
        public ReturnType ReturnType { get; set; }
    }
}
